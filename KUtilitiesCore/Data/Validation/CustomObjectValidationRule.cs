using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Regla personalizada a nivel de objeto.
    /// </summary>
    internal class CustomObjectValidationRule<T> : IValidationRule<T>
    {
        private readonly Action<T, ValidationContext<T>, List<ValidationFailure>> _validationAction;

        internal CustomObjectValidationRule(Action<T, ValidationContext<T>, List<ValidationFailure>> validationAction)
        {
            _validationAction = validationAction;
        }

        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
        {
            var failures = new List<ValidationFailure>();
            // Ejecuta la acción personalizada, que puede añadir fallos a la lista
            _validationAction(context.InstanceToValidate, context, failures);
            return failures;
        }
    }
}
