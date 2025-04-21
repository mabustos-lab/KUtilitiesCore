using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    public class Messenger : IMessenger
    {
        private static IMessenger _defaultInstance;

        private readonly object _registerLock;
        private static SynchronizationContext _ctx;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsStrictAction;

        private Messenger()
        {
            _registerLock = new object();
            _ctx = SynchronizationContext.Current;
            _recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
            _recipientsStrictAction = new Dictionary<Type, List<WeakActionAndToken>>();
        }

        /// <summary>
        /// Obtiene la instancia predeterminada de Messenger, que permite registrar y enviar mensajes de forma estática.
        /// </summary>
        public static IMessenger Default
        {
            get
            {
                if (_defaultInstance == null)
                    _defaultInstance = new Messenger();
                return _defaultInstance;
            }
        }

        /// <summary>
        /// Proporciona una forma de invalidar la instancia de Messenger.Default con una instancia personalizada, por
        /// ejemplo, con fines de pruebas unitarias.
        /// </summary>
        /// <param name="newMessenger"></param>
        public static void OverrideDefault(Messenger newMessenger) { _defaultInstance = newMessenger; }


        /// <summary>
        /// Establece la instancia predeterminada (estática) de Messenger en null.
        /// </summary>
        public static void Reset() { _defaultInstance = null; }

        /// <summary>
        /// Registra un destinatario para un tipo de mensaje <typeparamref name="TTypeMessage"/>. El parámetro de acción
        /// se ejecutará cuando se envíe un mensaje correspondiente. Consulte el parámetro receiveDerivedMessagesToo
        /// para obtener detalles sobre cómo se pueden recibir también los mensajes derivados de <typeparamref
        /// name="TTypeMessage"/> (o, si <typeparamref name="TTypeMessage"/> es una interfaz, mensajes que implementan
        /// <typeparamref name="TTypeMessage"/>). <para> El registro de un destinatario no crea una referencia dura a
        /// él, por lo que si se elimina este destinatario, no se produce ninguna pérdida de memoria.</para>
        /// </summary>
        /// <typeparam name="TTypeMessage">Tipo de mensaje para el que se registra el destinatario.</typeparam>
        /// <param name="recipient">El destinatario que recibirá los mensajes.</param>
        /// <param name="token">
        /// Un token para un canal de mensajería. Si un destinatario se registra con un token y un remitente envía un
        /// mensaje con el mismo token, este mensaje se entregará al destinatario. Otros destinatarios que no usaron un
        /// token al registrarse (o que usaron un token diferente) no recibirán el mensaje. Del mismo modo, los mensajes
        /// enviados sin ningún token, o con un token diferente, no se entregarán a ese destinatario.
        /// </param>
        /// <param name="receiveDerivedMessagesToo">
        /// Si es true, los tipos de mensajes derivados de <typeparamref name="TTypeMessage"/> también se transmitirán
        /// al destinatario. Por ejemplo, si SendOrderMessage y ExecuteOrderMessage derivan de OrderMessage, registrarse
        /// en OrderMessage y establecer receiveDerivedMessagesToo en true enviará SendOrderMessage y
        /// ExecuteOrderMessage al destinatario que se registró. <para> Además, si <typeparamref name="TTypeMessage"/>
        /// es una interfaz, los tipos de mensajes que implementan <typeparamref name="TTypeMessage"/> también se
        /// transmitirán al destinatario. Por ejemplo, si SendOrderMessage y ExecuteOrderMessage implementan
        /// IOrderMessage, registrarse en IOrderMessage y establecer receiveDerivedMessagesToo en true enviará
        /// SendOrderMessage y ExecuteOrderMessage al destinatario que se registró.</para>
        /// </param>
        /// <param name="action">
        /// La acción que se ejecutará cuando se envíe un mensaje de tipo TMessage.
        /// </param>
        public virtual void Register<TTypeMessage>(
            object recipient,
            Action<TTypeMessage> action,
            bool receiveDerivedMessagesToo = false,
            object token = null)
        {
            Dictionary<Type, List<WeakActionAndToken>> recipientList = GetRecipientList(receiveDerivedMessagesToo);

            lock (_registerLock)
            {
                Type messageType = typeof(TTypeMessage);

                List<WeakActionAndToken> currentList = GetOrAddRecipientList(recipientList, messageType);

                var weakActionAndToken = CreateWeakActionAndToken(recipient, action, token);
                currentList.Add(weakActionAndToken);
            }

            Cleanup();
        }

        /// <summary>
        /// Obtiene o crea una lista de destinatarios para el tipo especificado.
        /// </summary>
        private List<WeakActionAndToken> GetOrAddRecipientList(
            Dictionary<Type, List<WeakActionAndToken>> recipientList,
            Type type)
        {
            if (!recipientList.TryGetValue(type, out List<WeakActionAndToken> list))
            {
                list = new List<WeakActionAndToken>();
                recipientList[type] = list;
            }
            return list;
        }

        /// <summary>
        /// Crea la acción débil con su token.
        /// </summary>
        private WeakActionAndToken CreateWeakActionAndToken<TTypeMessage>(
            object recipient,
            Action<TTypeMessage> action,
            object token)
        { return new WeakActionAndToken() { Action = new WeakAction<TTypeMessage>(recipient, action), Token = token }; }
        /// <summary>
        /// Obtiene la lista de destinatarios según el tipo de registro.
        /// </summary>
        private Dictionary<Type, List<WeakActionAndToken>> GetRecipientList(bool includeSubclasses)
        { return includeSubclasses ? _recipientsOfSubclassesAction : _recipientsStrictAction; }


        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje llegará a todos los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <param name="message">El mensaje que se enviará a los destinatarios registrados.</param>
        public virtual void Send<TTypeMessage>(TTypeMessage message)
        { this.SendWithCriteria<TTypeMessage>(message, null, null); }

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje solo llegará a los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register y que son de targetType.
        /// </summary>
        /// <typeparam name="TTypeMessage">The type of message that will be sent.</typeparam>
        /// <typeparam name="TTarget">
        /// El tipo de destinatarios que recibirán el mensaje. El mensaje no se enviará a destinatarios de otro tipo.
        /// </typeparam>
        /// <param name="message">El mensaje que se enviará a los destinatarios registrados.</param>
        public virtual void Send<TTypeMessage, TTarget>(TTypeMessage message)
        { this.SendWithCriteria<TTypeMessage>(message, typeof(TTarget), null); }

        /// <summary>
        /// Envía un mensaje a los destinatarios registrados. El mensaje solo llegará a los destinatarios que se
        /// registraron para este tipo de mensaje mediante uno de los métodos Register y que son de targetType.
        /// </summary>
        /// <typeparam name="TTypeMessage">El tipo de mensaje que se enviará.</typeparam>
        /// <param name="message">El mensaje que se debe enviar a los destinatarios registrados.</param>
        /// <param name="token">
        /// Un token para un canal de mensajería. Si un destinatario se registra con un token y un remitente envía un
        /// mensaje con el mismo token, este mensaje se entregará al destinatario. Otros destinatarios que no usaron un
        /// token al registrarse (o que usaron un token diferente) no recibirán el mensaje. Del mismo modo, los mensajes
        /// enviados sin ningún token, o con un token diferente, no se entregarán a ese destinatario.
        /// </param>
        public virtual void Send<TTypeMessage>(TTypeMessage message, object token)
        { this.SendWithCriteria<TTypeMessage>(message, null, token); }

        private void SendWithCriteria<TTypeMessage>(TTypeMessage message, Type messageTargetType, object token)
        {
            Type typeMessage = typeof(TTypeMessage);
            //if (_recipientsOfSubclassesAction != null)
            //{
            //    List<Type> lstTypes = _recipientsOfSubclassesAction.Keys.ToList();
            //    foreach (Type type1 in lstTypes)
            //    {
            //        List<WeakActionAndToken> item = null;
            //        if (typeMessage == type1 || typeMessage.IsSubclassOf(type1) || Implements(typeMessage, type1))
            //        {
            //            item = _recipientsOfSubclassesAction[type1];
            //        }
            //        SendToList<TTypeMessage>(message, item, messageTargetType, token);
            //    }
            //}
            //if (_recipientsStrictAction != null)
            //{
            //    if (this._recipientsStrictAction.ContainsKey(typeMessage))
            //    {
            //        List<WeakActionAndToken> weakActionAndTokens =
            //            _recipientsStrictAction[typeMessage];
            //        SendToList<TTypeMessage>(message, weakActionAndTokens, messageTargetType, token);
            //    }
            //}
            List<WeakActionAndToken> actions = GetMessageActions(typeMessage);
            SendToList(message, actions, messageTargetType, token);
            Cleanup();
        }

        /// <summary>
        /// Obtiene las acciones asociadas a un tipo de mensaje.
        /// </summary>
        private List<WeakActionAndToken> GetMessageActions(Type messageType)
        {
            List<WeakActionAndToken> actions = GetActionsFromSubclasses(messageType);
            actions.AddRange(GetActionsFromStrictRegistry(messageType));
            return actions;
        }

        /// <summary>
        /// Obtiene acciones de la lista de clases derivadas.
        /// </summary>
        private List<WeakActionAndToken> GetActionsFromSubclasses(Type messageType)
        {
            List<WeakActionAndToken> actions = new List<WeakActionAndToken>();
            if (_recipientsOfSubclassesAction.ContainsKey(messageType))
            {
                actions.AddRange(_recipientsOfSubclassesAction[messageType]);
            }
            return actions;
        }

        /// <summary>
        /// Obtiene acciones de la lista estricta.
        /// </summary>
        private List<WeakActionAndToken> GetActionsFromStrictRegistry(Type messageType)
        {
            if (_recipientsStrictAction.TryGetValue(messageType, out List<WeakActionAndToken> actions))
            {
                return actions;
            }
            return new List<WeakActionAndToken>();
        }

        /// <summary>
        /// Cancela completamente el registro de un destinatario del mensajer. Después de ejecutar este método, el
        /// destinatario ya no recibirá ningún mensaje.
        /// </summary>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        public virtual void Unregister(object recipient)
        {
            Messenger.UnregisterFromLists(recipient, this._recipientsOfSubclassesAction);
            Messenger.UnregisterFromLists(recipient, this._recipientsStrictAction);
        }

        /// <summary>
        /// Anular el registro de un destinatario de mensaje solo para un tipo determinado de mensajes. Después de
        /// ejecutar este método, el destinatario ya no recibirá mensajes de tipo <typeparamref name="TTypeMessage"/>,
        /// pero seguirá recibiendo otros tipos de mensajes (si se registró para ellos anteriormente).
        /// </summary>
        /// <param name="recipient">El destinatario que debe estar dado de baja.</param>
        /// <typeparam name="TTypeMessage">
        /// El tipo de mensajes de los que el destinatario desea darse de baja.
        /// </typeparam>
        public virtual void Unregister<TTypeMessage>(object recipient) { Unregister<TTypeMessage>(recipient, null); }

        /// <summary>
        /// Unregisters a message recipient for a given type of messages only and for a given token.  After this method
        /// is executed, the recipient will not receive messages of type TMessage anymore with the given token, but will
        /// still receive other message types or messages with other tokens (if it registered for them previously).
        /// </summary>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="token">The token for which the recipient must be unregistered.</param>
        /// <typeparam name="TTypeMessage">The type of messages that the recipient wants to unregister from.</typeparam>
        public virtual void Unregister<TTypeMessage>(object recipient, object token)
        { Unregister<TTypeMessage>(recipient, token, null); }

        /// <summary>
        /// Unregisters a message recipient for a given type of messages and for a given action. Other message types
        /// will still be transmitted to the recipient (if it registered for them previously). Other actions that have
        /// been registered for the message type TMessage and for the given recipient (if available) will also remain
        /// available.
        /// </summary>
        /// <typeparam name="TTypeMessage">The type of messages that the recipient wants to unregister from.</typeparam>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="action">The action that must be unregistered for the recipient and for the message type TMessage.</param>
        public virtual void Unregister<TTypeMessage>(object recipient, Action<TTypeMessage> action)
        {
            Messenger.UnregisterFromLists<TTypeMessage>(recipient, action, this._recipientsStrictAction);
            Messenger.UnregisterFromLists<TTypeMessage>(recipient, action, this._recipientsOfSubclassesAction);
            Cleanup();
        }

        /// <summary>
        /// Unregisters a message recipient for a given type of messages, for a given action and a given token. Other
        /// message types will still be transmitted to the recipient (if it registered for them previously). Other
        /// actions that have been registered for the message type TMessage, for the given recipient and other tokens
        /// (if available) will also remain available.
        /// </summary>
        /// <typeparam name="TMessage">
        /// The type of messages that the recipient wants to unregister from.
        /// </typeparam>
        /// <param name="recipient">The recipient that must be unregistered.</param>
        /// <param name="token">The token for which the recipient must be unregistered.</param>
        /// <param name="action">The action that must be unregistered for the recipient and for the message type TMessage.</param>
        public virtual void Unregister<TTypeMessage>(object recipient, object token, Action<TTypeMessage> action)
        {
            Messenger.UnregisterFromLists<TTypeMessage>(recipient, token, action, this._recipientsStrictAction);
            Messenger.UnregisterFromLists<TTypeMessage>(recipient, token, action, this._recipientsOfSubclassesAction);
            Cleanup();
        }

        /// <summary>
        /// Limpia una lista de destinatarios removiendo entradas obsoletas.
        /// </summary>
        private static void CleanupList(IDictionary<Type, List<WeakActionAndToken>> recipientList)
        {
            List<Type> typesToRemove = new List<Type>();
            foreach (var entry in recipientList)
            {
                entry.Value.RemoveAll(item => item.Action == null || !item.Action.IsAlive);
                if (entry.Value.Count == 0)
                {
                    typesToRemove.Add(entry.Key);
                }
            }
            foreach (Type type in typesToRemove)
            {
                recipientList.Remove(type);
            }
        }

        /// <summary>
        /// Verifica si un tipo implementa una interfaz específica.
        /// </summary>
        private static bool Implements(Type instanceType, Type interfaceType)
        { return instanceType.IsAssignableFrom(interfaceType); }

        /// <summary>
        /// Envia un mensaje a una lista de destinatarios específicos.
        /// </summary>
        private static void SendToList<TTypeMessage>(
            TTypeMessage message,
            IEnumerable<WeakActionAndToken> actions,
            Type messageTargetType,
            object token)
        {
            foreach (WeakActionAndToken actionToken in actions)
            {
                if (ShouldSendMessage(actionToken, messageTargetType, token))
                {
                    ExecuteAction((IExecuteWithObject)actionToken.Action, message);
                }
            }
        }

        /// <summary>
        /// Verifica si un mensaje debe ser enviado a un destinatario específico.
        /// </summary>
        private static bool ShouldSendMessage(WeakActionAndToken actionToken, Type messageType, object token)
        {
            WeakAction action = actionToken.Action;
            return action != null &&
                action.IsAlive &&
                action.Target != null &&
                (messageType == null ||
                    action.Target.GetType() == messageType ||
                    Implements(action.Target.GetType(), messageType)) &&
                TokensMatch(actionToken.Token, token);
        }

        /// <summary>
        /// Verifica si los tokens coinciden.
        /// </summary>
        private static bool TokensMatch(object token1, object token2)
        { return (token1 == null && token2 == null) || (token1 != null && token1.Equals(token2)); }

        /// <summary>
        /// Ejecuta una acción con el mensaje proporcionado.
        /// </summary>
        private static void ExecuteAction(IExecuteWithObject action, object message)
        {
            if (_ctx != null)
            {
                _ctx.Send(_ => action.ExecuteWithObject(message), null);
            }
            else
            {
                action.ExecuteWithObject(message);
            }
        }

        private static void UnregisterFromLists(object recipient, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if ((recipient != null && lists != null && lists.Count != 0))
            {
                lock (lists)
                {
                    foreach (Type key in lists.Keys)
                    {
                        foreach (WeakActionAndToken item in lists[key])
                        {
                            WeakAction action = item.Action;
                            if (action != null && recipient == action.Target)
                            {
                                action.MarkForDeletion();
                            }
                        }
                    }
                }
            }
        }

        private static void UnregisterFromLists<TMessage>(
            object recipient,
            Action<TMessage> action,
            Dictionary<Type, List<Messenger.WeakActionAndToken>> lists)
        {
            foreach (Type key in lists.Keys.ToList())
            {
                lists[key] = RemoveDeadActions(lists[key], recipient);
                if (lists[key].Count == 0)
                {
                    lists.Remove(key);
                }
            }
        }
        private static void UnregisterFromLists<TMessage>(
            object recipient,
            object token,
            Action<TMessage> action,
            Dictionary<Type, List<Messenger.WeakActionAndToken>> lists)
        {
            bool flag;
            Type type = typeof(TMessage);
            if (recipient != null && lists != null && lists.Count != 0 && lists.ContainsKey(type))
            {
                foreach (WeakActionAndToken item in lists[type])
                {
                    flag = item.Action is WeakAction<TMessage> weakAction &&
                        recipient == weakAction.Target &&
                        (action == null || action == weakAction.Action) &&
                        (token == null || token.Equals(item.Token));
                    if (flag)
                    {
                        item.Action.MarkForDeletion();
                    }
                }

            }
        }
        /// <summary>
        /// Implementaciónde forEach element removal.ige removal.
        /// </summary>
        private static List<WeakActionAndToken> RemoveDeadActions(List<WeakActionAndToken> actions,
            object recipient)
        { return actions.Where(action => !IsActionDead(action.Action, recipient)).ToList(); }
        /// <summary>
        /// Verifica si una acción debe ser removida.
        /// </summary>
        private static bool IsActionDead(WeakAction action, object recipient)
        { return action == null || !action.IsAlive || action.Target != recipient; }


        private void Cleanup()
        {
            CleanupList(this._recipientsOfSubclassesAction);
            CleanupList(this._recipientsStrictAction);
        }


        private struct WeakActionAndToken
        {
            public WeakAction Action;

            public object Token;
        }
    }
}