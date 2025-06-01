using KUtilitiesCore.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace KUtilitiesCore.Data.ValidationAttributes
{
    /// <summary>
    /// Validates that the annotated property's date value is less than another specified property's value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateLessThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonPropertyName;
        private string _comparisonPropertyDisplayName = string.Empty;
        private readonly bool _nullAsMaxValue;

        /// <summary>
        /// Configura la validación comparando con otra propiedad
        /// </summary>
        /// <param name="comparisonPropertyName">Nombre de la propiedad de comparación</param>
        /// <param name="allowedEquality">Si se permite igualdad entre fechas</param>
        /// <param name="nullAsMaxValue">Tratar null como DateTime.MaxValue</param>
        public DateLessThanAttribute(
            string comparisonPropertyName,
            bool allowedEquality = false,
            bool nullAsMaxValue = false)
        {
            _comparisonPropertyName = comparisonPropertyName ?? throw new ArgumentNullException(nameof(comparisonPropertyName));
            _nullAsMaxValue = nullAsMaxValue;
            SetErrorMessage(allowedEquality);
        }

        /// <summary>
        /// Indica si se permite igualdad entre los valores comparados
        /// </summary>
        public bool AllowedEquality { get; }

        public override bool RequiresValidationContext => true;

        private void SetErrorMessage(bool allowedEqualit)
        {
            ErrorMessage = allowedEqualit ?
                ValidationAtrributesStrings.ValidationLessThanOrEqualToError :
                ValidationAtrributesStrings.ValidationLessThanError;
        }

        /// <inheritdoc/>
        public override string FormatErrorMessage(string name)
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                ErrorMessageString,
                _comparisonPropertyDisplayName,
                name
            );
        }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if ((validationContext.MemberName ?? string.Empty)
                .Equals(_comparisonPropertyName, StringComparison.OrdinalIgnoreCase))
            {
                return new ValidationResult(
                    string.Format(ValidationAtrributesStrings.ValidationSamePropertyError, nameof(DateGreaterThanAttribute)));
            }

            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonPropertyName);
            if (comparisonProperty is null)
            {
                return new ValidationResult(
                    string.Format(ValidationAtrributesStrings.ValidatiomPropertyNotFound, _comparisonPropertyName));
            }

            _comparisonPropertyDisplayName = comparisonProperty.DataAnnotationsDisplayName() ?? _comparisonPropertyName;

            var currentValue = ParseDateTime(value, validationContext.DisplayName);
            var comparisonValue = ParseDateTime(
                comparisonProperty.GetValue(validationContext.ObjectInstance),
                _comparisonPropertyDisplayName
            );

            if (currentValue == DateTime.MinValue || comparisonValue == DateTime.MinValue)
                return ValidationResult.Success;

            if (ShouldReturnError(currentValue, comparisonValue))
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        private DateTime ParseDateTime(object? value, string displayName)
        {
            if (value is null && !_nullAsMaxValue) return DateTime.MaxValue;

            var stringValue = value?.ToString() ?? (_nullAsMaxValue ? DateTime.MaxValue.ToString() : string.Empty);

            if (!DateTime.TryParse(stringValue, CultureInfo.CurrentCulture,
                DateTimeStyles.None, out DateTime parsedDate))
            {
                throw new ValidationException(
                    string.Format(ValidationAtrributesStrings.ValidationIsNotDateTypeError, displayName));
            }

            return parsedDate;
        }

        private bool ShouldReturnError(DateTime current, DateTime comparison)
        {
            return AllowedEquality ?
                current >= comparison :
                current > comparison;
        }
    }
}