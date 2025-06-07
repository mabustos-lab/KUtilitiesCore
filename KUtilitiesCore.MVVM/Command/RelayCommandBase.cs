using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.MVVM.Command
{
    /// <summary>
    /// Clase base abstracta para comandos Relay, implementando IViewModelCommand y lógica común.
    /// </summary>
    public abstract class RelayCommandBase : IViewModelCommand
    {
        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RelayCommandBase"/>.
        /// </summary>
        protected RelayCommandBase()
        {
        }

        #endregion Constructors

        #region Events

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        #endregion Events

        #region Properties

        /// <inheritdoc/>
        public virtual string CommandName { get; internal set; } = string.Empty;

        /// <inheritdoc/>
        public virtual bool IsParametrizedCommand { get; internal set; }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public abstract bool CanExecute(object? parameter);

        /// <inheritdoc/>
        public abstract void Execute(object? parameter);

        /// <summary>
        /// Obtiene el parametro asociado a el view model si se requiere en el comando.
        /// </summary>
        public virtual object? GetViewModelParameter(object viewModelSource) => null;

        /// <summary>
        /// Notifica a la interfaz de usuario que el estado de ejecución del comando ha cambiado.
        /// </summary>
        public virtual void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Valida que la expresión sea una llamada a método con la firma esperada.
        /// </summary>
        /// <param name="expression">Expresión lambda.</param>
        /// <param name="expectedReturnType">Tipo de retorno esperado.</param>
        /// <param name="expectedParameterCount">Cantidad de parámetros esperada.</param>
        /// <exception cref="ArgumentException">Si la expresión no cumple con la firma esperada.</exception>
        internal static void ValidateMethodExpression(
            LambdaExpression expression,
            Type expectedReturnType,
            int expectedParameterCount)
        {
            if (expression.Body is not MethodCallExpression methodCall)
            {
                throw new ArgumentException("La expresión debe ser una llamada a método.", nameof(expression));
            }

            if (methodCall.Method.ReturnType != expectedReturnType)
            {
                throw new ArgumentException($"El método debe retornar {expectedReturnType.Name}.", nameof(expression));
            }

            if (methodCall.Method.GetParameters().Length != expectedParameterCount)
            {
                throw new ArgumentException($"Se requieren {expectedParameterCount} parámetro(s).", nameof(expression));
            }
        }

        /// <summary>
        /// Valida que la expresión sea un MemberExpression sobre el ViewModel.
        /// </summary>
        /// <param name="expression">Expresión lambda.</param>
        /// <param name="viewModelType">Tipo del ViewModel esperado.</param>
        /// <exception cref="ArgumentException">
        /// Si la expresión no es un MemberExpression sobre el ViewModel.
        /// </exception>
        internal static void ValidateViewModelMemberExpression(
            LambdaExpression expression,
            Type viewModelType)
        {
            if (expression.Body is not MemberExpression memberExpr)
                throw new ArgumentException("La expresión debe ser un acceso a miembro (propiedad o campo) del ViewModel.", nameof(expression));

            if (memberExpr.Expression is not ParameterExpression paramExpr || paramExpr.Type != viewModelType)
                throw new ArgumentException("La expresión debe referenciar un miembro del ViewModel.", nameof(expression));
        }

        /// <summary>
        /// Indica si existe lógica de ejecución asociada al comando.
        /// </summary>
        /// <returns>True si existe lógica de ejecución; de lo contrario, false.</returns>
        internal abstract bool HasExecuteLogic();

        /// <summary>
        /// Inicializa los metadatos del comando, como el nombre y si es parametrizado.
        /// </summary>
        /// <param name="expression">Expresión lambda del método.</param>
        /// <param name="expectedParameters">Cantidad esperada de parámetros.</param>
        internal void InitializeCommandMetadata(LambdaExpression expression, int expectedParameters)
        {
            if (expression.Body is MethodCallExpression methodCall)
            {
                CommandName = methodCall.Method.Name;
            }

            IsParametrizedCommand = expectedParameters > 0;
        }

        #endregion Methods
    }
}