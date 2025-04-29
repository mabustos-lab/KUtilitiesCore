using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Representa una regla asociada a una propiedad específica.
    /// Contiene una lista de validadores específicos para esa propiedad.
    /// </summary>
    internal class PropertyRule<T, TProperty> : IValidationRule<T>
    {
        public string PropertyName { get; }
        private readonly Func<T, TProperty> _propertyFunc;
        private readonly List<IPropertyValidator<T, TProperty>> _validators = new List<IPropertyValidator<T, TProperty>>();

        internal PropertyRule(string propertyName, Func<T, TProperty> propertyFunc)
        {
            PropertyName = propertyName;
            _propertyFunc = propertyFunc;
        }

        internal void AddValidator(IPropertyValidator<T, TProperty> validator)
        {
            _validators.Add(validator);
        }

        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
        {
            TProperty propertyValue = _propertyFunc(context.InstanceToValidate);
            var failures = new List<ValidationFailure>();

            foreach (var validator in _validators)
            {
                if (!validator.IsValid(context, propertyValue))
                {
                    failures.Add(new ValidationFailure(
                        PropertyName,
                        validator.GetErrorMessage(context, propertyValue), // Mensaje de error específico del validador
                        propertyValue // Valor que causó el fallo
                    ));
                    // Aquí también se podría implementar CascadeMode para la propiedad
                    // if (CascadeMode == CascadeMode.StopOnFirstFailure) break;
                }
            }
            return failures;
        }
    }
}
