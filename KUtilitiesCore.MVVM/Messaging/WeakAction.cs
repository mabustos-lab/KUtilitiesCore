using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Almacena una <see cref="Action"/> sin crear una referencia fuerte a su propietario,
    /// permitiendo que el propietario sea recolectado por el GC.
    /// </summary>
    internal class WeakAction
    {
        private readonly Action? _action;
        private readonly WeakReference _targetReference;

        /// <summary>
        /// Obtiene un valor que indica si el propietario de la acción (Target) sigue vivo.
        /// </summary>
        public bool IsAlive => _targetReference != null && _targetReference.IsAlive;

        /// <summary>
        /// Obtiene el propietario de la acción. Devuelve <c>null</c> si ha sido recolectado.
        /// </summary>
        public object? Target => _targetReference?.Target;

        /// <summary>
        /// Obtiene la acción almacenada como un delegado base.
        /// </summary>
        protected virtual Delegate ActionHandlerDelegate => _action;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="WeakAction"/>.
        /// </summary>
        /// <param name="target">El propietario de la acción.</param>
        /// <param name="action">La acción a almacenar.</param>
        public WeakAction(object target, Action action) // Acepta Action (no genérico)
        {
            _targetReference = new WeakReference(target);
            _action = action;
        }

        /// <summary>
        /// Ejecuta la acción si el propietario sigue vivo.
        /// </summary>
        public void Execute()
        {
            if (_action != null && IsAlive)
            {
                _action();
            }
        }
    }

    /// <summary>
    /// Almacena una <see cref="Action{T}"/> sin crear una referencia fuerte a su propietario.
    /// </summary>
    /// <typeparam name="T">El tipo del parámetro de la acción.</typeparam>
    internal class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        private readonly Action<T> _typedAction;

        /// <summary>
        /// Obtiene la acción tipada almacenada.
        /// </summary>
        public Action<T> TypedActionHandler => _typedAction;

        /// <inheritdoc/>
        protected override Delegate ActionHandlerDelegate => _typedAction;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="WeakAction{T}"/>.
        /// </summary>
        /// <param name="target">El propietario de la acción.</param>
        /// <param name="action">La acción tipada a almacenar.</param>
        public WeakAction(object target, Action<T> action)
            : base(target, null) // La acción base no genérica no se usa directamente aquí
        {
            _typedAction = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Ejecuta la acción con el valor predeterminado de <typeparamref name="T"/> si el propietario sigue vivo.
        /// </summary>
        public new void Execute() // Oculta el Execute base
        {
            if (_typedAction != null && IsAlive)
            {
                _typedAction(default);
            }
        }

        /// <summary>
        /// Ejecuta la acción con el parámetro proporcionado si el propietario sigue vivo.
        /// </summary>
        /// <param name="parameter">El parámetro para la acción.</param>
        public void Execute(T parameter)
        {
            if (_typedAction != null && IsAlive)
            {
                _typedAction(parameter);
            }
        }

        /// <summary>
        /// Ejecuta la acción con un parámetro de tipo object, que será casteado a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="parameter">El parámetro para la acción.</param>
        public void ExecuteWithObject(object parameter)
        {
            if (_typedAction != null && IsAlive)
            {
                if (parameter is T typedParameter)
                {
                    _typedAction(typedParameter);
                }
                else if (parameter == null && !typeof(T).IsValueType) // Permite null para tipos de referencia
                {
                    _typedAction(default); // default(T) será null para tipos de referencia
                }
                else
                {
                    // Considerar lanzar InvalidCastException o notificar error si el casteo falla y es crítico.
                    // Por ahora, no se ejecuta si el casteo no es posible (excepto null para ref types).
                    // Console.WriteLine($"WeakAction: Type mismatch. Expected {typeof(T)}, got {parameter?.GetType()}.");
                }
            }
        }
    }
}
