using KUtilitiesCore.Data.Validation.Core;
using System.Linq.Expressions;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Clase base para crear validadores específicos para un tipo T.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto a validar.</typeparam>
    public abstract class AbstractValidator<T> : IValidator<T>
    {
        #region Fields

        // Almacena reglas específicas de propiedades
        private readonly List<IValidationRule<T>> _rules = [];

        #endregion Fields

        #region Methods

        /// <summary>
        /// Valida la instancia proporcionada ejecutando todas las reglas registradas. Implementa el
        /// patrón Chain of Responsibility de forma implícita: las reglas se ejecutan en el orden en
        /// que se añadieron. Por defecto, se ejecutan todas las reglas (CascadeMode.Continue).
        /// </summary>
        /// <param name="instance">La instancia a validar.</param>
        /// <returns>El resultado de la validación.</returns>
        public ValidationResult Validate(T instance)
        {
            var context = new ValidationContext<T>(instance);
            return Validate(context);
        }

        /// <summary>
        /// Valida la instancia usando el contexto proporcionado.
        /// </summary>
        /// <param name="context">El contexto de validación.</param>
        /// <returns>El resultado de la validación.</returns>
        public virtual ValidationResult Validate(ValidationContext<T> context)
        {
            var result = new ValidationResult();

            // Ejecutar reglas de propiedad
            foreach (var rule in _rules)
            {
                var failures = rule.Validate(context);
                result.AddFailures(failures);
                // Aquí se podría implementar CascadeMode.StopOnFirstFailure si se desea if
                // (!result.IsValid && CascadeMode == CascadeMode.StopOnFirstFailure) return result;
            }

            return result;
        }

        /// <summary>
        /// Define una regla de validación personalizada a nivel de objeto. Útil para validaciones
        /// que involucran múltiples propiedades.
        /// </summary>
        /// <param name="customValidationAction">La acción que contiene la lógica de validación.</param>
        protected void Custom(Action<T, ValidationContext<T>, List<ValidationFailure>> customValidationAction)
        {
            if (customValidationAction == null) throw new ArgumentNullException(nameof(customValidationAction));
            _rules.Add(new CustomObjectValidationRule<T>(customValidationAction));
        }

        /// <summary>
        /// Define reglas de validación para una propiedad específica.
        /// </summary>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="expression">
        /// Expresión lambda para seleccionar la propiedad (ej: x =&gt; x.Nombre).
        /// </param>
        /// <returns>Un RuleBuilder para encadenar reglas.</returns>
        protected IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("La expresión debe ser una propiedad o campo.", nameof(expression));

            string propertyName = memberExpression.Member.Name;
            var compiledExpression = expression.Compile(); // Compila para obtener el valor

            var rule = new PropertyRule<T, TProperty>(propertyName, compiledExpression);
            _rules.Add(rule); // Añade la regla base de propiedad

            // Devuelve el builder para encadenar reglas específicas (NotNull, Length, etc.)
            return new RuleBuilder<T, TProperty>(rule);
        }

        #endregion Methods
    }
}