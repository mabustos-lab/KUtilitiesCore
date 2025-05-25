using System;
using System.Linq;
#if NET8_0_OR_GREATER
using System.Threading.Channels;
#else
using System.Collections.Concurrent;
using System.Threading.Tasks;
#endif

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Implementación del servicio de mensajería <see cref="IMessenger"/>.
    /// Permite la comunicación desacoplada mediante el envío y recepción de mensajes.
    /// </summary>
    public class Messenger : IMessenger, IDisposable
    {
        private static IMessenger _defaultInstance;
        private static readonly object _defaultInstanceLock = new();

        private readonly object _subscriptionsLock = new();
        private readonly SynchronizationContext _capturedSyncContext;

        // Almacena suscripciones donde el tipo de mensaje debe coincidir exactamente.
        private readonly Dictionary<Type, List<Subscription>> _strictSubscriptions;
        // Almacena suscripciones donde se aceptan tipos derivados o implementaciones de interfaz.
        private readonly Dictionary<Type, List<Subscription>> _derivedSubscriptions;

#if NET8_0_OR_GREATER
        private readonly Channel<MessageEnvelope> _messageQueue;
#else
        private readonly BlockingCollection<MessageEnvelope> _messageQueue;
        private readonly Task _processingTask;
#endif
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1); // Limpiar cada minuto por defecto
        private const int ModificationsCleanupThreshold = 20; // Limpiar después de N modificaciones
        private int _modificationsSinceLastCleanup ;

        /// <summary>
        /// Ocurre cuando ocurre una excepción durante la ejecución de la acción de un suscriptor.
        /// </summary>
        public event EventHandler<MessengerErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Obtiene la instancia predeterminada (Singleton) del Messenger.
        /// </summary>
        public static IMessenger Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (_defaultInstanceLock)
                    {
                        if (_defaultInstance == null) // Doble verificación de bloqueo
                        {
                            _defaultInstance = new Messenger();
                        }
                    }
                }
                return _defaultInstance;
            }
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Messenger"/>.
        /// </summary>
        public Messenger()
        {
            _strictSubscriptions = new Dictionary<Type, List<Subscription>>();
            _derivedSubscriptions = new Dictionary<Type, List<Subscription>>();
            _capturedSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _cancellationTokenSource = new CancellationTokenSource();

#if NET8_0_OR_GREATER
            _messageQueue = Channel.CreateUnbounded<MessageEnvelope>(new UnboundedChannelOptions { SingleReader = true });
            Task.Run(() => ProcessMessageQueueAsync(_cancellationTokenSource.Token));
#else
            _messageQueue = new BlockingCollection<MessageEnvelope>(new ConcurrentQueue<MessageEnvelope>());
            _processingTask = Task.Factory.StartNew(() => ProcessMessageQueue(_cancellationTokenSource.Token),
                                                   _cancellationTokenSource.Token,
                                                   TaskCreationOptions.LongRunning,
                                                   TaskScheduler.Default);
#endif
            // Iniciar temporizador para limpieza periódica.
            // El estado 'this' se pasa para que el delegado pueda llamar a Cleanup en esta instancia.
            _cleanupTimer = new Timer(state => ((Messenger)state!).Cleanup(), this, _cleanupInterval, _cleanupInterval);
        }

        /// <summary>
        /// Permite reemplazar la instancia predeterminada del Messenger, útil para pruebas unitarias.
        /// </summary>
        /// <param name="newMessenger">La nueva instancia del Messenger a usar como predeterminada. Si es null, se lanzará ArgumentNullException.</param>
        public static void OverrideDefault(IMessenger newMessenger)
        {
            lock (_defaultInstanceLock)
            {
                _defaultInstance = newMessenger ?? throw new ArgumentNullException(nameof(newMessenger));
            }
        }

        /// <summary>
        /// Restablece la instancia predeterminada del Messenger a null.
        /// La próxima vez que se acceda a `Default`, se creará una nueva instancia de `Messenger`.
        /// </summary>
        public static void Reset()
        {
            lock (_defaultInstanceLock)
            {
                if (_defaultInstance is IDisposable disposable)
                {
                    disposable.Dispose(); // Disponer la instancia anterior si es IDisposable
                }
                _defaultInstance = null;
            }
        }

        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            RegisterInternal(recipient, null, false, action);
        }

        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            RegisterInternal(recipient, null, receiveDerivedMessagesToo, action);
        }

        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            RegisterInternal(recipient, token, false, action);
        }

        /// <inheritdoc/>
        public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            RegisterInternal(recipient, token, receiveDerivedMessagesToo, action);
        }

        private void RegisterInternal<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            if (recipient == null) throw new ArgumentNullException(nameof(recipient));
            if (action == null) throw new ArgumentNullException(nameof(action));

            Type messageType = typeof(TMessage);
            var weakAction = new WeakAction<TMessage>(recipient, action);
            var subscription = new Subscription(weakAction, token);

            lock (_subscriptionsLock)
            {
                var subscriptionsList = receiveDerivedMessagesToo ? GetOrCreateSubscriptionList(_derivedSubscriptions, messageType)
                                                              : GetOrCreateSubscriptionList(_strictSubscriptions, messageType);
                subscriptionsList.Add(subscription);
                RequestCleanupIfNeeded();
            }
        }

        private List<Subscription> GetOrCreateSubscriptionList(Dictionary<Type, List<Subscription>> dictionary, Type messageType)
        {
            // Asume que ya se tiene el _subscriptionsLock
            if (!dictionary.TryGetValue(messageType, out var list))
            {
                list = new List<Subscription>();
                dictionary[messageType] = list;
            }
            return list;
        }

        /// <inheritdoc/>
        public void Send<TMessage>(TMessage message)
        {
            SendInternalEnvelope(message, null, null);
        }

        /// <inheritdoc/>
        public void Send<TMessage, TTarget>(TMessage message)
        {
            SendInternalEnvelope(message, typeof(TTarget), null);
        }

        /// <inheritdoc/>
        public void Send<TMessage>(TMessage message, object token)
        {
            SendInternalEnvelope(message, null, token);
        }

        private void SendInternalEnvelope<TMessage>(TMessage messageContent, Type targetTypeFilter, object token)
        {
            if (messageContent is null) 
                throw new ArgumentNullException(nameof(messageContent));

            Type declaredMessageType = typeof(TMessage);
            Type actualMessageType = messageContent.GetType();

            var envelope = new MessageEnvelope(messageContent, actualMessageType, targetTypeFilter, token, declaredMessageType);

#if NET8_0_OR_GREATER
            if (!_messageQueue.Writer.TryWrite(envelope))
            {
                // Log o manejo de error si la escritura falla (improbable con UnboundedChannel).
                OnErrorOccurred(null, envelope.MessageContent, new InvalidOperationException("Failed to write message to queue."));
            }
#else
            try
            {
                _messageQueue.Add(envelope, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) { /* La adición fue cancelada, la cola se está cerrando */ }
            catch (Exception ex) // Otras excepciones de Add, ej. si la colección está marcada como completa y es acotada.
            {
                OnErrorOccurred(null, envelope.MessageContent, new InvalidOperationException("Failed to add message to queue.", ex));
            }
#endif
        }

#if NET8_0_OR_GREATER
        private async Task ProcessMessageQueueAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var envelope in _messageQueue.Reader.ReadAllAsync(cancellationToken))
                {
                    ProcessSingleEnvelope(envelope);
                }
            }
            catch (OperationCanceledException) { /* Comportamiento esperado al cerrar. */ }
            catch (Exception ex)
            {
                OnErrorOccurred(null, null, new Exception("Unhandled error in message processing loop.", ex));
            }
        }
#else
        private void ProcessMessageQueue(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var envelope in _messageQueue.GetConsumingEnumerable(cancellationToken))
                {
                    ProcessSingleEnvelope(envelope);
                }
            }
            catch (OperationCanceledException) { /* Comportamiento esperado al cerrar. */ }
            catch (Exception ex)
            {
                OnErrorOccurred(null, null, new Exception("Unhandled error in message processing loop.", ex));
            }
        }
#endif

        private void ProcessSingleEnvelope(MessageEnvelope envelope)
        {
            var recipientsToExecute = new HashSet<Subscription>(); // Usar HashSet para evitar duplicados

            lock (_subscriptionsLock)
            {
                // Procesar suscripciones estrictas
                if (_strictSubscriptions.TryGetValue(envelope.DeclaredMessageType, out var strictList))
                {
                    foreach (var sub in strictList)
                    {
                        if (sub.IsAliveAndMatches(envelope.Token, envelope.TargetTypeFilter))
                            recipientsToExecute.Add(sub);
                    }
                }

                // Procesar suscripciones que aceptan tipos derivados/implementaciones
                foreach (var entry in _derivedSubscriptions)
                {
                    Type subscribedType = entry.Key;
                    // Comprueba herencia e implementación de interfaz (ActualMessageType hereda/implementa subscribedType)
                    if (subscribedType.IsAssignableFrom(envelope.ActualMessageType))
                    {
                        foreach (var sub in entry.Value)
                        {
                            if (sub.IsAliveAndMatches(envelope.Token, envelope.TargetTypeFilter))
                                recipientsToExecute.Add(sub);
                        }
                    }
                }
            }

            foreach (var sub in recipientsToExecute)
            {
                try
                {
                    if (envelope.MessageContent is IConditionalMessage conditionalMessage &&
                        sub.Action.Target != null &&
                        !conditionalMessage.ShouldProcess(sub.Action.Target))
                    {
                        continue;
                    }
                    sub.Execute(envelope.MessageContent, _capturedSyncContext, OnErrorOccurred);
                }
                catch (Exception ex) // Captura excepciones directas de sub.Execute si no usa el errorHandler
                {
                    OnErrorOccurred(sub.Action.Target, envelope.MessageContent, ex);
                }
            }
        }

        /// <inheritdoc/>
        public void Unregister(object recipient)
        {
            if (recipient == null) return; // No hacer nada si el destinatario es null
            lock (_subscriptionsLock)
            {
                RemoveFromAllSubscriptions(sub => sub.Action.Target == recipient);
                RequestCleanupIfNeeded();
            }
        }

        private void RemoveFromAllSubscriptions(Predicate<Subscription> match)
        {
            // Asume que ya se tiene el _subscriptionsLock
            CleanupDictionary(_strictSubscriptions, match);
            CleanupDictionary(_derivedSubscriptions, match);
        }


        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null, null);
        }

        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient, object token)
        {
            Unregister<TMessage>(recipient, token, null);
        }

        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            Unregister(recipient, null, action);
        }

        /// <inheritdoc/>
        public void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            // Al menos uno debe ser no nulo para evitar desregistrar todo accidentalmente
            if (recipient == null && token == null && action == null)
            {
                // Podría ser una operación válida para limpiar todas las suscripciones de un tipo TMessage,
                // pero es potencialmente peligrosa. Se podría requerir una confirmación o un método explícito.
                // Por ahora, si todos son null, no hacemos nada o lanzamos excepción.
                // throw new ArgumentException("At least one of recipient, token, or action must be non-null to unregister.");
                return;
            }

            Type messageType = typeof(TMessage);
            lock (_subscriptionsLock)
            {
                Predicate<Subscription> match = sub =>
                {
                    bool recipientMatch = recipient == null || sub.Action.Target == recipient;
                    if (!recipientMatch) return false;

                    bool tokenMatch = token == null && sub.Token == null || token != null && token.Equals(sub.Token);
                    if (!tokenMatch) return false;

                    bool actionMatch = action == null;
                    if (!actionMatch && sub.Action is WeakAction<TMessage> typedWeakAction)
                    {
                        actionMatch = typedWeakAction.TypedActionHandler.Equals(action);
                    }
                    else if (!actionMatch && action != null) // action especificada pero WeakAction no es del tipo correcto
                    {
                        return false;
                    }
                    return actionMatch;
                };

                CleanupDictionary(_strictSubscriptions, messageType, match);
                CleanupDictionary(_derivedSubscriptions, messageType, match);
                // Nota: Si TMessage es una interfaz, y se registró un tipo concreto que implementa TMessage
                // en _derivedSubscriptions, la clave del diccionario sería la interfaz TMessage.
                // Si se registró un tipo derivado, la clave sería el tipo base.
                // Esta lógica de CleanupDictionary(type, match) funciona bien para estos casos.
                RequestCleanupIfNeeded();
            }
        }

        private void RequestCleanupIfNeeded()
        {
            // Asume que se llama dentro de un lock de _subscriptionsLock si es necesario para _modificationsSinceLastCleanup
            // Pero _modificationsSinceLastCleanup puede ser interlocked si se accede fuera del lock principal.
            // Por simplicidad, se asume que se llama desde un contexto ya bloqueado o que el riesgo es bajo.
            Interlocked.Increment(ref _modificationsSinceLastCleanup);
            if (_modificationsSinceLastCleanup >= ModificationsCleanupThreshold)
            {
                // Ejecutar en ThreadPool para no bloquear el hilo actual si Cleanup es largo.
                // El lock dentro de Cleanup manejará la concurrencia.
                ThreadPool.QueueUserWorkItem(state => ((Messenger)state).Cleanup(), this);
                Interlocked.Exchange(ref _modificationsSinceLastCleanup, 0);
            }
        }

        /// <summary>
        /// Limpia las suscripciones inactivas (cuyo Target ha sido recolectado por el GC).
        /// </summary>
        private void Cleanup()
        {
            lock (_subscriptionsLock)
            {
                CleanupDictionary(_strictSubscriptions, sub => !sub.IsAlive);
                CleanupDictionary(_derivedSubscriptions, sub => !sub.IsAlive);
            }
        }

        private void CleanupDictionary(Dictionary<Type, List<Subscription>> dictionary, Predicate<Subscription> matchToRemove)
        {
            // Asume que ya se tiene el _subscriptionsLock
            if (dictionary == null || dictionary.Count == 0) return;

            var typesWithEmptyLists = new List<Type>();
            foreach (var entry in dictionary)
            {
                entry.Value.RemoveAll(matchToRemove);
                if (entry.Value.Count == 0)
                {
                    typesWithEmptyLists.Add(entry.Key);
                }
            }

            foreach (Type typeKey in typesWithEmptyLists)
            {
                dictionary.Remove(typeKey);
            }
        }

        private void CleanupDictionary(Dictionary<Type, List<Subscription>> dictionary, Type messageTypeFilter, Predicate<Subscription> matchToRemove)
        {
            // Asume que ya se tiene el _subscriptionsLock
            if (dictionary == null || !dictionary.TryGetValue(messageTypeFilter, out var subscriptions))
            {
                return;
            }

            subscriptions.RemoveAll(matchToRemove);
            if (subscriptions.Count == 0)
            {
                dictionary.Remove(messageTypeFilter);
            }
        }

        private void OnErrorOccurred(object? recipient, object? message, Exception error)
        {
            ErrorOccurred?.Invoke(this, new MessengerErrorEventArgs(recipient, message, error));
            // Considerar loggear aquí también, independientemente de si hay suscriptores al evento.
            // System.Diagnostics.Debug.WriteLine($"Messenger Error: Recipient='{recipient}', Message='{message}', Error='{error}'");
        }

        /// <summary>
        /// Libera los recursos utilizados por el Messenger, deteniendo el procesamiento de mensajes.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos.
        /// </summary>
        /// <param name="disposing">True si se llama desde Dispose(), false si se llama desde el finalizador.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cleanupTimer?.Dispose();
                _cancellationTokenSource.Cancel();
#if NET8_0_OR_GREATER
                _messageQueue.Writer.TryComplete();
#else
                _messageQueue.CompleteAdding();
                try
                {
                    // Esperar un tiempo prudencial a que la tarea de procesamiento termine.
                    _processingTask?.Wait(TimeSpan.FromSeconds(2));
                }
                catch (OperationCanceledException) { /* Esperado */ }
                catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OperationCanceledException || e is TaskCanceledException)) { /* Esperado */ }
                _messageQueue?.Dispose();
#endif
                _cancellationTokenSource?.Dispose();
            }
        }

        /// <summary>
        /// Representa un mensaje en la cola interna del Messenger.
        /// </summary>
        private class MessageEnvelope
        {
            /// <summary>El contenido del mensaje.</summary>
            public object MessageContent { get; }
            /// <summary>El tipo real del mensaje en tiempo de ejecución.</summary>
            public Type ActualMessageType { get; }
            /// <summary>El tipo declarado del mensaje al momento del envío (puede ser una clase base o interfaz).</summary>
            public Type DeclaredMessageType { get; }
            /// <summary>El tipo de destino opcional para el mensaje.</summary>
            public Type TargetTypeFilter { get; }
            /// <summary>El token opcional asociado con el mensaje.</summary>
            public object Token { get; }

            public MessageEnvelope(object messageContent, Type actualMessageType, Type targetTypeFilter, object token, Type declaredMessageType)
            {
                MessageContent = messageContent;
                ActualMessageType = actualMessageType;
                TargetTypeFilter = targetTypeFilter;
                Token = token;
                DeclaredMessageType = declaredMessageType;
            }
        }

        /// <summary>
        /// Representa una suscripción individual a un mensaje.
        /// </summary>
        private sealed class Subscription
        {
            public WeakAction Action { get; }
            public object Token { get; }

            public Subscription(WeakAction action, object token)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));
                Token = token;
            }

            public bool IsAlive => Action.IsAlive;

            public bool IsAliveAndMatches(object messageToken, Type messageTargetType)
            {
                if (!IsAlive) return false;
                // Comprobar token
                bool tokenMatch = Token == null && messageToken == null || Token != null && Token.Equals(messageToken);
                if (!tokenMatch) return false;

                // Comprobar tipo de target
                if (messageTargetType != null)
                {
                    object? targetInstance = Action.Target;
                    if (targetInstance == null || !messageTargetType.IsInstanceOfType(targetInstance))
                    {
                        return false;
                    }
                }
                return true;
            }

            public void Execute(object messageContent, SynchronizationContext syncContext, Action<object?, object?, Exception> errorHandler)
            {
                if (!IsAlive) return;

                if (Action is IExecuteWithObject executableAction)
                {
                    try
                    {
                        if (syncContext != null && syncContext != SynchronizationContext.Current)
                        {
                            syncContext.Post(state =>
                            {
                                try
                                {
                                    executableAction.ExecuteWithObject(state);
                                }
                                catch (Exception ex)
                                {
                                    errorHandler?.Invoke(Action.Target, state, ex);
                                }
                            }, messageContent);
                        }
                        else
                        {
                            executableAction.ExecuteWithObject(messageContent);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorHandler?.Invoke(Action.Target, messageContent, ex);
                    }
                }
            }

            public override bool Equals(object? obj)
                => obj is Subscription other
                    && ReferenceEquals(Action, other.Action)
                    && (Token == null && other.Token == null || Token?.Equals(other.Token) == true);

            public override int GetHashCode()
            {
#if NET8_0_OR_GREATER
                return HashCode.Combine(Action, Token);
#else
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (Action != null ? Action.GetHashCode() : 0);
                    hash = hash * 23 + (Token != null ? Token.GetHashCode() : 0);
                    return hash;
                }
#endif
            }
        }
    }
}
