using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.ValidationAttributes
{
    /// <summary>
    /// Indica que una propiedad es requerida si cumple una condición
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfAttribute(string otherProperty, object targetValue) : ValidationAttribute
    {

        /// <summary>
        /// Indica la propiedad de la cual se requere el valor para que cumpla la condición
        /// </summary>
        public string OtherProperty { get; private set; } = otherProperty;

        /// <inheritdoc/>
        public override bool RequiresValidationContext => true;

        /// <summary>
        /// Indica el valor que debe cumplir la condición
        /// </summary>
        public object TargetValue { get; private set; } = targetValue;

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherPropertyValue = validationContext.ObjectType
                                                  .GetProperty(OtherProperty)?
                                                  .GetValue(validationContext.ObjectInstance);
            if (otherPropertyValue is null
                || !otherPropertyValue.Equals(TargetValue))
                return ValidationResult.Success;
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(ErrorMessage ?? "Este campo es requerido.");
            }

            return ValidationResult.Success;
        }

    }
}
