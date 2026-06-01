using System;
using System.Reflection;
using KUtilitiesCore.MVVM.Command;

namespace KUtilitiesCore.MVVM.Command.Binder
{
    /// <summary>
    /// Enlaza de forma segura y desechable un evento de un objeto a un IViewModelCommand usando reflexión.
    /// </summary>
    /// <typeparam name="T">
    /// El tipo del objeto que contiene el evento. Debe ser un tipo de referencia.
    /// </typeparam>
    internal sealed class EventCommandBinder<T> : CommandBinderBase<T> where T : class
    {
        private readonly Delegate _eventHandler;
        private readonly EventInfo _eventInfo;

        public EventCommandBinder(T targetObject, string eventName,
            Action<bool> targetStatus, IViewModelCommand command)
            : base(targetObject, targetStatus, command)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            _eventInfo = TargetObject.GetType().GetEvent(eventName)
                         ?? throw new ArgumentException($"El evento '{eventName}' no fue encontrado en el tipo '{typeof(T).Name}'.", nameof(eventName));

            _eventHandler = Delegate.CreateDelegate(
                _eventInfo.EventHandlerType!,
                this,
                nameof(OnEventTriggered));
        }

        protected override void Subscribe()
        {
            _eventInfo.AddEventHandler(TargetObject, _eventHandler);
        }

        protected override void Unsubscribe()
        {
            _eventInfo.RemoveEventHandler(TargetObject, _eventHandler);
        }

        private void OnEventTriggered(object sender, EventArgs e)
        {
            object? parameter = Command.IsParametrizedCommand ? Command.GetViewModelParameter() : null;
            if (Command.CanExecute(parameter))
            {
                Command.Execute(parameter);
            }
        }
    }
}
