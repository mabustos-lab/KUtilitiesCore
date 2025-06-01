using KUtilitiesCore.Data.Validation.Core;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Regla personalizada a nivel de objeto.
    /// </summary>
    internal class CustomObjectValidationRule<T> : IValidationRule<T>
    {
        #region Fields

        private readonly Action<T, ValidationContext<T>, List<ValidationFailure>> _validationAction;

        #endregion Fields

        #region Constructors

        internal CustomObjectValidationRule(Action<T, ValidationContext<T>, List<ValidationFailure>> validationAction)
        {
            _validationAction = validationAction;
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
        {
            var failures = new List<ValidationFailure>();
            // Ejecuta la acción personalizada, que puede añadir fallos a la lista
            _validationAction(context.InstanceToValidate, context, failures);
            return failures;
        }

        #endregion Methods
    }
}