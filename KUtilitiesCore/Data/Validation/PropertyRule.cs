using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static KUtilitiesCore.Data.Validation.RuleBuilderExtensions;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Representa una regla asociada a una propiedad específica.
    /// Contiene una lista de validadores específicos para esa propiedad y sus mensajes personalizados.
    /// </summary>
    internal class PropertyRule<T, TProperty> : IValidationRule<T>
    {
        public string PropertyName { get; }
        private readonly Func<T, TProperty> _propertyFunc;
        // Almacena tuplas de (Validador, MensajePersonalizadoOpcional)
        private readonly List<(IPropertyValidator<T, TProperty> validator, string customMessageFormat)> _validators = new();

        internal PropertyRule(string propertyName, Func<T, TProperty> propertyFunc)
        {
            PropertyName = propertyName;
            _propertyFunc = propertyFunc;
        }

        /// <summary>
        /// Añade un validador a la regla de propiedad.
        /// </summary>
        internal void AddValidator(IPropertyValidator<T, TProperty> validator)
        {
            _validators.Add((validator, string.Empty)); // Inicialmente sin mensaje personalizado
        }

        /// <summary>
        /// Establece un mensaje personalizado para el último validador añadido.
        /// </summary>
        internal void SetMessageForLastValidator(string messageFormat)
        {
            if (_validators.Any())
            {
                var lastIndex = _validators.Count - 1;
                var lastValidatorTuple = _validators[lastIndex];
                // Actualiza la tupla en la lista con el nuevo mensaje
                _validators[lastIndex] = (lastValidatorTuple.validator, messageFormat);
            }
            else
            {
                // Podría lanzar una excepción si se llama a WithMessage antes de añadir una regla.
                // O simplemente ignorarlo. Por ahora, lo ignora.
                Console.WriteLine("Advertencia: Se llamó a WithMessage sin una regla previa.");
            }
        }

        /// <summary>
        /// Ejecuta todos los validadores asociados a esta propiedad.
        /// </summary>
        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
        {
            TProperty propertyValue = _propertyFunc(context.InstanceToValidate);
            var failures = new List<ValidationFailure>();

            foreach (var (validator, customMessageFormat) in _validators)
            {
                if (!validator.IsValid(context, propertyValue))
                {
                    // Determina qué plantilla de mensaje usar (personalizada o por defecto)
                    string messageTemplate = customMessageFormat ?? validator.GetErrorMessage(context, propertyValue);

                    // Formatea la plantilla reemplazando los placeholders
                    string formattedMessage = FormatMessageTemplate(messageTemplate, validator, context, propertyValue, PropertyName);

                    failures.Add(new ValidationFailure(
                        PropertyName, -1,
                        formattedMessage,
                        propertyValue // Valor que causó el fallo
                    ));

                    if (ValidationOptions.Instance.CascadeMode == ValidationMode.StopOnFirstFailure)
                        break;
                }
            }
            return failures;
        }

        /// <summary>
        /// Formatea una plantilla de mensaje reemplazando placeholders conocidos.
        /// </summary>
        private static string FormatMessageTemplate(string template, IPropertyValidator<T, TProperty> validator,
            ValidationContext<T> context, TProperty propertyValue, string propertyName)
        {
            
            // Reemplazos básicos comunes a todas las reglas
            var formatted = template
                .Replace("{PropertyName}", propertyName ?? "<Property>")
                .Replace("{PropertyValue}", propertyValue?.ToString() ?? "<null>"); // Usa el operador null-coalescing para evitar problemas con valores nulos

            // Reemplazos específicos basados en el tipo de validador
            switch (validator)
            {
                case LengthValidator<T> lengthValidator:
                    formatted = formatted
                        .Replace("{MinLength}", lengthValidator.Min.ToString())
                        .Replace("{MaxLength}", lengthValidator.Max.ToString());
                    break;
                case GreaterThanValidator<T, TProperty> gtValidator:
                    formatted = formatted.Replace("{ComparisonValue}", gtValidator.ValueToCompare!.ToString());
                    break;
                case LessThanValidator<T, TProperty> ltValidator:
                    // Asegúrate de que TProperty es un tipo de valor no anulable antes de usarlo
                    formatted = formatted.Replace("{ComparisonValue}", ltValidator.ValueToCompare!.ToString());
                    break;
                case AllowedValuesValidator<T, TProperty> avValidator:
                    formatted = formatted.Replace("{AllowedValuesDescription}", avValidator.AllowedValueDefinition.GetAllowedDescription());
                    break;
                    /*
                     Añadir más casos para otros validadores con placeholders específicos
                     case RegularExpressionValidator<T> regexValidator:
                        formatted = formatted.Replace("{Pattern}", regexValidator.Pattern);
                        break;
                    */
            }

            // Podrían añadirse más placeholders globales si es necesario (ej: {InstanceId})

            return formatted;
        }
    }
}