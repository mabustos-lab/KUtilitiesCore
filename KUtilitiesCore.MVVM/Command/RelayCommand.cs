using System;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;

namespace KUtilitiesCore.MVVM.Command
{
    /// <summary>
    /// Interfaz que extiende ICommand para exponer metadatos adicionales de comandos en ViewModels.
    /// </summary>
    public interface IViewModelCommand : ICommand
    {
        /// <summary>
        /// Nombre del método asociado al comando.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Indica si el comando requiere un parámetro.
        /// </summary>
        public bool IsParametrizedCommand { get; }
    }

    /// <summary>
    /// Comando genérico que permite enlazar métodos con parámetros en un ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel asociado.</typeparam>
    /// <typeparam name="TParam">Tipo del parámetro del comando.</typeparam>
    public class RelayCommand<TViewModel, TParam> : RelayCommandBase where TViewModel : class
    {
        /// <summary>
        /// Función que determina si el comando puede ejecutarse con el parámetro proporcionado.
        /// </summary>
        private Func<TParam?, bool>? _canExecuteWithParamFunc = null;

        /// <summary>
        /// Acción que se ejecuta cuando se invoca el comando con parámetro.
        /// </summary>
        private Action<TParam?>? _executeWithParamAction = null;

        /// <summary>
        /// Crea un comando que ejecuta un método con parámetro en el ViewModel.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="executeExpression">Expresión que representa el método a ejecutar con parámetro.</param>
        /// <param name="parameterPropertyExpression">
        /// Expresión que representa la propiedad del parámetro.
        /// </param>
        /// <param name="canExecuteExpression">
        /// Expresión opcional que determina si el comando puede ejecutarse con parámetro.
        /// </param>
        /// <returns>Instancia de RelayCommand configurada.</returns>
        /// <exception cref="ArgumentNullException">Si viewModel o executeExpression son nulos.</exception>
        public static RelayCommand<TViewModel, TParam> Create(
            TViewModel viewModel,
            Expression<Action<TViewModel, TParam?>> executeExpression,
            Expression<Func<TViewModel, TParam?>> parameterPropertyExpression,
            Expression<Func<TViewModel, TParam?, bool>>? canExecuteExpression = null)
        {
            if(viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if(executeExpression == null)
                throw new ArgumentNullException(nameof(executeExpression));
            RelayCommand<TViewModel, TParam> relayCommand = new();
            RelayCommandBase.ValidateViewModelMemberExpression(parameterPropertyExpression,typeof(TViewModel));
            relayCommand.InitializeCommandMetadata(executeExpression, expectedParameters: 1);
            relayCommand.CompileParametrizedExecuteLogic(viewModel, executeExpression);
            relayCommand.CompileParametrizedCanExecuteLogic(viewModel, canExecuteExpression);
            return relayCommand;
        }

        /// <summary>
        /// Determina si el comando puede ejecutarse con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        /// <returns>True si puede ejecutarse; de lo contrario, false.</returns>
        public override bool CanExecute(object? parameter)
        { return _canExecuteWithParamFunc?.Invoke((TParam?)parameter) ?? HasExecuteLogic(); }

        /// <summary>
        /// Ejecuta el comando con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        public override void Execute(object? parameter) { _executeWithParamAction?.Invoke((TParam?)parameter); }

        /// <summary>
        /// Indica si existe lógica de ejecución asociada al comando.
        /// </summary>
        /// <returns>True si existe lógica de ejecución; de lo contrario, false.</returns>
        internal override bool HasExecuteLogic() => _executeWithParamAction != null;

        /// <summary>
        /// Compila la lógica de validación para comandos con parámetro.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="canExecuteExpression">Expresión de validación.</param>
        private void CompileParametrizedCanExecuteLogic(
            TViewModel viewModel,
            Expression<Func<TViewModel, TParam?, bool>>? canExecuteExpression)
        {
            if(canExecuteExpression == null)
                return;

            ValidateMethodExpression(canExecuteExpression, typeof(bool), 1);
            var canExecuteDelegate = canExecuteExpression.Compile();
            _canExecuteWithParamFunc = param => canExecuteDelegate(viewModel, param);
        }

        /// <summary>
        /// Compila la lógica de ejecución para comandos con parámetro.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="executeExpression">Expresión de ejecución.</param>
        private void CompileParametrizedExecuteLogic(
            TViewModel viewModel,
            Expression<Action<TViewModel, TParam?>> executeExpression)
        {
            ValidateMethodExpression(executeExpression, typeof(void), 1);
            var executeDelegate = executeExpression.Compile();
            _executeWithParamAction = param => executeDelegate(viewModel, param);
        }
        
    }

    /// <summary>
    /// Implementación genérica de ICommand para ViewModels, permitiendo enlazar métodos y condiciones de ejecución sin
    /// parámetros.
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel asociado.</typeparam>
    public class RelayCommand<TViewModel> : RelayCommandBase where TViewModel : class
    {
        /// <summary>
        /// Función que determina si el comando puede ejecutarse.
        /// </summary>
        private Func<bool>? _canExecuteFunc = null;

        /// <summary>
        /// Acción que se ejecuta cuando se invoca el comando.
        /// </summary>
        private Action? _executeAction = null;

        /// <summary>
        /// Crea un comando que ejecuta un método sin parámetros en el ViewModel.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="executeExpression">Expresión que representa el método a ejecutar.</param>
        /// <param name="canExecuteExpression">
        /// Expresión opcional que determina si el comando puede ejecutarse.
        /// </param>
        /// <returns>Instancia de RelayCommand configurada.</returns>
        /// <exception cref="ArgumentNullException">Si viewModel o executeExpression son nulos.</exception>
        public static RelayCommand<TViewModel> Create(
            TViewModel viewModel,
            Expression<Action<TViewModel>> executeExpression,
            Expression<Func<TViewModel, bool>>? canExecuteExpression = null)
        {
            if(viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if(executeExpression == null)
                throw new ArgumentNullException(nameof(executeExpression));
            RelayCommand<TViewModel> relayCommand = new();
            relayCommand.InitializeCommandMetadata(executeExpression, expectedParameters: 0);
            relayCommand.CompileExecuteLogic(viewModel, executeExpression);
            relayCommand.CompileCanExecuteLogic(viewModel, canExecuteExpression);
            return relayCommand;
        }

        /// <summary>
        /// Determina si el comando puede ejecutarse con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        /// <returns>True si puede ejecutarse; de lo contrario, false.</returns>
        public override bool CanExecute(object? parameter) { return _canExecuteFunc?.Invoke() ?? HasExecuteLogic(); }

        /// <summary>
        /// Ejecuta el comando con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        public override void Execute(object? parameter) { _executeAction?.Invoke(); }

        /// <summary>
        /// Indica si existe lógica de ejecución asociada al comando.
        /// </summary>
        /// <returns>True si existe lógica de ejecución; de lo contrario, false.</returns>
        internal override bool HasExecuteLogic() => _executeAction != null;

        /// <summary>
        /// Compila la lógica de validación para comandos sin parámetros.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="canExecuteExpression">Expresión de validación.</param>
        private void CompileCanExecuteLogic(
            TViewModel viewModel,
            Expression<Func<TViewModel, bool>>? canExecuteExpression)
        {
            if(canExecuteExpression == null)
                return;

            ValidateMethodExpression(canExecuteExpression, typeof(bool), 0);
            var canExecuteDelegate = canExecuteExpression.Compile();
            _canExecuteFunc = () => canExecuteDelegate(viewModel);
        }

        /// <summary>
        /// Compila la lógica de ejecución para comandos sin parámetros.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="executeExpression">Expresión de ejecución.</param>
        private void CompileExecuteLogic(TViewModel viewModel, Expression<Action<TViewModel>> executeExpression)
        {
            ValidateMethodExpression(executeExpression, typeof(void), 0);
            var executeDelegate = executeExpression.Compile();
            _executeAction = () => executeDelegate(viewModel);
        }
    }

    /// <summary>
    /// Clase base abstracta para comandos Relay, implementando IViewModelCommand y lógica común.
    /// </summary>
    public abstract class RelayCommandBase : IViewModelCommand
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RelayCommandBase"/>.
        /// </summary>
        protected RelayCommandBase()
        {
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc/>
        public virtual string CommandName { get; internal set; } = string.Empty;

        /// <inheritdoc/>
        public virtual bool IsParametrizedCommand { get; internal set; }

        /// <inheritdoc/>
        public abstract bool CanExecute(object? parameter);

        /// <inheritdoc/>
        public abstract void Execute(object? parameter);

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
            if(expression.Body is not MethodCallExpression methodCall)
            {
                throw new ArgumentException("La expresión debe ser una llamada a método.", nameof(expression));
            }

            if(methodCall.Method.ReturnType != expectedReturnType)
            {
                throw new ArgumentException($"El método debe retornar {expectedReturnType.Name}.", nameof(expression));
            }

            if(methodCall.Method.GetParameters().Length != expectedParameterCount)
            {
                throw new ArgumentException($"Se requieren {expectedParameterCount} parámetro(s).", nameof(expression));
            }
        }
        /// <summary>
        /// Valida que la expresión sea un MemberExpression sobre el ViewModel.
        /// </summary>
        /// <param name="expression">Expresión lambda.</param>
        /// <param name="viewModelType">Tipo del ViewModel esperado.</param>
        /// <exception cref="ArgumentException">Si la expresión no es un MemberExpression sobre el ViewModel.</exception>
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
            if(expression.Body is MethodCallExpression methodCall)
            {
                CommandName = methodCall.Method.Name;
            }

            IsParametrizedCommand = expectedParameters > 0;
        }
    }
}