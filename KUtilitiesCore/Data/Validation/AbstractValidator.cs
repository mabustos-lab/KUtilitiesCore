using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Clase base para crear validadores específicos para un tipo T.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto a validar.</typeparam>
    public abstract class AbstractValidator<T> : IValidator<T>
    {
        // Almacena reglas específicas de propiedades
        private readonly List<IValidationRule<T>> _propertyRules = new List<IValidationRule<T>>();
        // Almacena reglas a nivel de objeto (personalizadas)
        private readonly List<IValidationRule<T>> _customObjectRules = new List<IValidationRule<T>>();

        /// <summary>
        /// Define reglas de validación para una propiedad específica.
        /// </summary>
        /// <typeparam name="TProperty">El tipo de la propiedad.</typeparam>
        /// <param name="expression">Expresión lambda para seleccionar la propiedad (ej: x => x.Nombre).</param>
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
            _propertyRules.Add(rule); // Añade la regla base de propiedad

            // Devuelve el builder para encadenar reglas específicas (NotNull, Length, etc.)
            return new RuleBuilder<T, TProperty>(rule);
        }

        /// <summary>
        /// Define una regla de validación personalizada a nivel de objeto.
        /// Útil para validaciones que involucran múltiples propiedades.
        /// </summary>
        /// <param name="customValidationAction">La acción que contiene la lógica de validación.</param>
        protected void Custom(Action<T, ValidationContext<T>, List<ValidationFailure>> customValidationAction)
        {
            if (customValidationAction == null) throw new ArgumentNullException(nameof(customValidationAction));
            _customObjectRules.Add(new CustomObjectValidationRule<T>(customValidationAction));
        }

        /// <summary>
        /// Valida la instancia proporcionada ejecutando todas las reglas registradas.
        /// Implementa el patrón Chain of Responsibility de forma implícita:
        /// las reglas se ejecutan en el orden en que se añadieron.
        /// Por defecto, se ejecutan todas las reglas (CascadeMode.Continue).
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
            foreach (var rule in _propertyRules)
            {
                var failures = rule.Validate(context);
                result.AddFailures(failures);
                // Aquí se podría implementar CascadeMode.StopOnFirstFailure si se desea
                // if (!result.IsValid && CascadeMode == CascadeMode.StopOnFirstFailure) return result;
            }

            // Ejecutar reglas personalizadas a nivel de objeto
            foreach (var rule in _customObjectRules)
            {
                var failures = rule.Validate(context);
                result.AddFailures(failures);
                // if (!result.IsValid && CascadeMode == CascadeMode.StopOnFirstFailure) return result;
            }

            return result;
        }
    }
}
