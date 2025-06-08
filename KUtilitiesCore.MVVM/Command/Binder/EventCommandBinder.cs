using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KUtilitiesCore.MVVM.Command.Binder
{
    /// <summary>
    /// Enlaza de forma segura y desechable un evento de un objeto a un IViewModelCommand.
    /// Su única responsabilidad es gestionar la suscripción a un evento para ejecutar un comando,
    /// previniendo fugas de memoria sin gestionar directamente el estado de la UI.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto que contiene el evento. Debe ser un tipo de referencia.</typeparam>
    sealed class EventCommandBinder<T> : IDisposable where T : class
    {

        private readonly T _targetObject;
        private readonly IViewModelCommand _command;
        private readonly EventInfo _eventInfo;
        private readonly Delegate _eventHandler;
        private bool _isDisposed;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EventCommandBinder{T}"/>.
        /// </summary>
        /// <param name="targetObject">El objeto que expone el evento (ej. un control de UI).</param>
        /// <param name="eventExpression">Una expresión lambda que identifica el evento a enlazar (ej. c => c.Click).</param>
        /// <param name="command">El comando que se ejecutará cuando el evento se dispare.</param>
        /// <exception cref="ArgumentNullException">Se lanza si targetObject, eventExpression o command son nulos.</exception>
        /// <exception cref="ArgumentException">Se lanza si la expresión no apunta a un evento válido en el objeto de destino.</exception>
        public EventCommandBinder(T targetObject, Expression<Func<T, Delegate>> eventExpression, IViewModelCommand command)
        {
            _targetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            _command = command ?? throw new ArgumentNullException(nameof(command));

            if (eventExpression == null)
            {
                throw new ArgumentNullException(nameof(eventExpression));
            }

            string eventName = GetEventNameFromExpression(eventExpression);
            _eventInfo = _targetObject.GetType().GetEvent(eventName)
                         ?? throw new ArgumentException($"El evento '{eventName}' no fue encontrado en el tipo '{typeof(T).Name}'.", nameof(eventExpression));

            // Se crea un delegado al manejador de eventos de instancia.
            // Esto es más limpio y seguro que usar reflexión para buscar el método por nombre.
            _eventHandler = Delegate.CreateDelegate(
                _eventInfo.EventHandlerType!,
                this,
                nameof(OnEventTriggered));

            _eventInfo.AddEventHandler(_targetObject, _eventHandler);
        }

        /// <summary>
        /// Manejador de eventos que se invoca cuando el objeto de destino dispara el evento enlazado.
        /// </summary>
        private void OnEventTriggered(object sender, EventArgs e)
        {
            // La lógica para determinar el parámetro y si se puede ejecutar está encapsulada aquí.
            object? parameter = _command.IsParametrizedCommand ? _command.GetViewModelParameter() : null;

            if (_command.CanExecute(parameter))
            {
                _command.Execute(parameter);
            }
        }

        /// <summary>
        /// Extrae el nombre del miembro (evento) de la expresión lambda de forma segura.
        /// </summary>
        private static string GetEventNameFromExpression(Expression<Func<T, Delegate>> expression)
        {
            if (expression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("La expresión debe ser una expresión de miembro que apunte a un evento.", nameof(expression));
            }
            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Libera los recursos y, crucialmente, desuscribe el manejador de eventos para evitar fugas de memoria.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _eventInfo.RemoveEventHandler(_targetObject, _eventHandler);
            _isDisposed = true;
        }

    }
}