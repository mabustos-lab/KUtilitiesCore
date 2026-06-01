using System;
using KUtilitiesCore.MVVM.Command;

namespace KUtilitiesCore.MVVM.Command.Binder
{
    /// <summary>
    /// Enlaza de forma segura y desechable un comando a un objeto mediante el uso de acciones de suscripción y desuscripción manuales.
    /// Útil para controles de terceros que requieren una firma de delegado específica o un proceso de enlace no basado en reflexión estándar.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto de destino.</typeparam>
    /// <typeparam name="THandler">El tipo del delegado esperado por el evento del objeto de destino.</typeparam>
    internal sealed class ManualCommandBinder<T, THandler> : CommandBinderBase<T> where T : class
    {
        private readonly Action<T, THandler> _subscribeAction;
        private readonly Action<T, THandler> _unsubscribeAction;
        private readonly THandler _handler;

        public ManualCommandBinder(T targetObject, Action<T, THandler> subscribe, Action<T, THandler> unsubscribe, 
            Action<bool>? targetStatus, IViewModelCommand command)
            : base(targetObject, targetStatus, command)
        {
            _subscribeAction = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            _unsubscribeAction = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));

            _handler = CreateHandler();
            
            base.Initialize(); // Explicitly call initialize to trigger Subscribe()
        }

        protected override void Subscribe()
        {
            _subscribeAction(TargetObject, _handler);
        }

        protected override void Unsubscribe()
        {
            _unsubscribeAction(TargetObject, _handler);
        }

        private THandler CreateHandler()
        {
            var method = typeof(ManualCommandBinder<T, THandler>).GetMethod(nameof(ExecuteInternal), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method == null)
            {
                throw new InvalidOperationException("No se encontró el método ExecuteInternal.");
            }

            try
            {
                return (THandler)(object)Delegate.CreateDelegate(typeof(THandler), this, method);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCastException($"El tipo de manejador {typeof(THandler).Name} no es compatible con la firma de ExecuteInternal (object, object).", ex);
            }
        }


        private void ExecuteInternal(object sender, object e)
        {
            object? parameter = Command.IsParametrizedCommand ? Command.GetViewModelParameter() : null;
            if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }
    }
}
