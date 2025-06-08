using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;

namespace KUtilitiesCore.MVVM.Command
{
    /// <summary>
    /// Comando genérico que permite enlazar métodos con parámetros en un ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel asociado.</typeparam>
    /// <typeparam name="TParam">Tipo del parámetro del comando.</typeparam>
    public class RelayCommand<TViewModel, TParam> : RelayCommandBase where TViewModel : class
    {
        #region Fields

        /// <summary>
        /// Función que determina si el comando puede ejecutarse con el parámetro proporcionado.
        /// </summary>
        private Func<TParam?, bool>? _canExecuteWithParamFunc;

        /// <summary>
        /// Acción que se ejecuta cuando se invoca el comando con parámetro.
        /// </summary>
        private Action<TParam?>? _executeWithParamAction;

        private Func<TParam?> _getParamDelegate = () => default;

        #endregion Fields

        #region Properties

        // <summary>
        /// Obtiene el nombre de la propiedad del ViewModel que este comando observa como parámetro.
        /// Es null si el comando no tiene parámetros o no se especificó una propiedad de parámetro. </summary>
        public string? WatchedParameterPropertyName { get; private set; }

        /// <summary>
        /// Obtiene el tipo de la propiedad del ViewModel que este comando observa como parámetro.
        /// Es null si el comando no tiene parámetros o no se especificó una propiedad de parámetro.
        /// </summary>
        public Type? WatchedParameterPropertyType { get; private set; }

        #endregion Properties

        #region Methods

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
            Expression<Func<TViewModel, TParam>> parameterPropertyExpression,
            Expression<Func<TViewModel, TParam?, bool>>? canExecuteExpression = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (executeExpression == null)
                throw new ArgumentNullException(nameof(executeExpression));
            RelayCommand<TViewModel, TParam> relayCommand = new();
            relayCommand.InitializeMemberMetaData(viewModel, parameterPropertyExpression);
            relayCommand.InitializeCommandMetadata(executeExpression, expectedParameters: 1);
            relayCommand.CompileParametrizedExecuteLogic(viewModel, executeExpression);
            relayCommand.CompileParametrizedCanExecuteLogic(viewModel, canExecuteExpression);
            if (viewModel is ISupportCommands viewModelHelper)
                viewModelHelper.RegisterCommand(relayCommand);
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
        public override void Execute(object? parameter)
        { _executeWithParamAction?.Invoke((TParam?)parameter); }

        /// <inheritdoc/>
        public override object? GetViewModelParameter()
        {
            return _getParamDelegate.Invoke();
        }

        /// <summary>
        /// Indica si existe lógica de ejecución asociada al comando.
        /// </summary>
        /// <returns>True si existe lógica de ejecución; de lo contrario, false.</returns>
        internal override bool HasExecuteLogic() => _executeWithParamAction != null;

        private static string? GetMemberNameFromExpression(Expression<Func<TViewModel, TParam>> expression)
        {
            Expression expressionBody = expression.Body;
            // Si la propiedad es un tipo de valor y la expresión la convierte a object (ej. vm =>
            // (object)vm.MyIntProp), el MemberExpression estará dentro de un UnaryExpression (Convert).
            if (expressionBody is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (unaryExpression.Operand is MemberExpression innerMemberExpression)
                {
                    return innerMemberExpression.Member.Name;
                }
            }
            else if (expressionBody is MemberExpression memberExpression)
            {
                // Esto cubre propiedades de tipos de referencia y tipos de valor que no se convierten.
                return memberExpression.Member.Name;
            }
            // Podría no ser una expresión de miembro simple (ej. vm => vm.MyObject.Property), en
            // cuyo caso este método simple no funcionará. Se podría hacer más robusto o lanzar excepción.
            Debug.WriteLine($"RelayCommand: La expresión '{expression}' no parece ser una expresión de miembro simple para obtener un nombre de propiedad.");
            return null;
        }

        /// <summary>
        /// Compila la lógica de validación para comandos con parámetro.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel.</param>
        /// <param name="canExecuteExpression">Expresión de validación.</param>
        private void CompileParametrizedCanExecuteLogic(
            TViewModel viewModel,
            Expression<Func<TViewModel, TParam?, bool>>? canExecuteExpression)
        {
            if (canExecuteExpression == null)
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

        private void InitializeMemberMetaData(TViewModel viewModel,
            Expression<Func<TViewModel, TParam>> parameterPropertyExpression)
        {
            ValidateViewModelMemberExpression(parameterPropertyExpression, typeof(TViewModel));
            var paramGetterDelegate = parameterPropertyExpression.Compile();
            _getParamDelegate = () => paramGetterDelegate(viewModel);
            WatchedParameterPropertyName = GetMemberNameFromExpression(parameterPropertyExpression);
            WatchedParameterPropertyType = typeof(TParam);
        }

        #endregion Methods
    }

    /// <summary>
    /// Implementación genérica de ICommand para ViewModels, permitiendo enlazar métodos y
    /// condiciones de ejecución sin parámetros.
    /// </summary>
    /// <typeparam name="TViewModel">Tipo del ViewModel asociado.</typeparam>
    public class RelayCommand<TViewModel> : RelayCommandBase where TViewModel : class
    {
        #region Fields

        /// <summary>
        /// Función que determina si el comando puede ejecutarse.
        /// </summary>
        private Func<bool>? _canExecuteFunc;

        /// <summary>
        /// Acción que se ejecuta cuando se invoca el comando.
        /// </summary>
        private Action? _executeAction;

        #endregion Fields

        #region Methods

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
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (executeExpression == null)
                throw new ArgumentNullException(nameof(executeExpression));
            RelayCommand<TViewModel> relayCommand = new();
            relayCommand.InitializeCommandMetadata(executeExpression, expectedParameters: 0);
            relayCommand.CompileExecuteLogic(viewModel, executeExpression);
            relayCommand.CompileCanExecuteLogic(viewModel, canExecuteExpression);
            if (viewModel is ISupportCommands viewModelHelper)
                viewModelHelper.RegisterCommand(relayCommand);
            return relayCommand;
        }

        /// <summary>
        /// Determina si el comando puede ejecutarse con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        /// <returns>True si puede ejecutarse; de lo contrario, false.</returns>
        public override bool CanExecute(object? parameter)
        { return _canExecuteFunc?.Invoke() ?? HasExecuteLogic(); }

        /// <summary>
        /// Ejecuta el comando con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">Parámetro opcional para el comando.</param>
        public override void Execute(object? parameter)
        { _executeAction?.Invoke(); }

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
            if (canExecuteExpression == null)
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

        #endregion Methods
    }
}