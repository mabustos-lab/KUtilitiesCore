using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text.RegularExpressions;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Métodos de extensión para el API Fluido (NotNull, NotEmpty, Must, etc.)
    /// </summary>
    public static class RuleBuilderExtensions
    {

        /// <summary>
        /// Añade una regla que valida si el valor está dentro del conjunto permitido definido por IRuleAllowedValue.
        /// </summary>
        public static IRuleBuilder<T, TProperty> AllowedValues<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder,
            IRuleAllowedValue<TProperty> allowedValueDefinition)
        {
            if (allowedValueDefinition == null) throw new ArgumentNullException(nameof(allowedValueDefinition));

            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new AllowedValuesValidator<T, TProperty>(allowedValueDefinition));
            return builder;
        }

        /// <summary>
        /// Añade una regla que valida que un valor numérico sea mayor que un valor específico.
        /// </summary>
        public static IRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueToCompare)
             where TProperty : struct, IComparable<TProperty>
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new GreaterThanValidator<T, TProperty>(valueToCompare));
            return builder;
        }

        /// <summary>
        /// Añade una regla que valida la longitud de una cadena.
        /// </summary>
        public static IRuleBuilder<T, string> Length<T>(this IRuleBuilder<T, string> ruleBuilder, int min, int max)
        {
            var builder = (RuleBuilder<T, string>)ruleBuilder;
            builder.AddValidator(new LengthValidator<T>(min, max));
            return builder;
        }

        /// <summary>
        /// Añade una regla que valida que un valor numérico sea menor que un valor específico.
        /// </summary>
        public static IRuleBuilder<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueToCompare)
            where TProperty : struct, IComparable<TProperty>
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new LessThanValidator<T, TProperty>(valueToCompare));
            return builder;
        }

        /// <summary>
        /// Añade una regla que valida usando una expresión regular.
        /// </summary>
        public static IRuleBuilder<T, string> Matches<T>(this IRuleBuilder<T, string> ruleBuilder, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            var builder = (RuleBuilder<T, string>)ruleBuilder;
            builder.AddValidator(new RegularExpressionValidator<T>(pattern, options));
            return builder;
        }

        /// <summary>
        /// Añade una regla personalizada basada en un predicado.
        /// </summary>
        public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder,
            Func<TProperty, bool> predicate,
            string errorMessageFormat = "La propiedad '{PropertyName}' no cumple la condición.")
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new PredicateValidator<T, TProperty>(predicate, errorMessageFormat));
            return builder;
        }

        /// <summary>
        /// Añade una regla personalizada basada en un predicado que involucra al objeto completo.
        /// </summary>
        public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder,
            Func<T, TProperty, bool> predicate, // Predicado que recibe la instancia completa y el valor de la propiedad
            string errorMessageFormat = "La propiedad '{PropertyName}' no cumple la condición basada en el objeto.")
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new InstancePredicateValidator<T, TProperty>(predicate, errorMessageFormat));
            return builder;
        }

        /// <summary>
        /// Añade una regla que asegura que la cadena no sea nula ni vacía.
        /// </summary>
        public static IRuleBuilder<T, string> NotEmpty<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
        {
            var builder = (RuleBuilder<T, string>)ruleBuilder;
            builder.AddValidator(new NotEmptyValidator<T>());
            return builder;
        }

        /// <summary>
        /// Añade una regla que asegura que la colección no sea nula ni vacía.
        /// </summary>
        public static IRuleBuilder<T, TCollection> NotEmpty<T, TCollection>(this IRuleBuilderInitial<T, TCollection> ruleBuilder)
            where TCollection : class, System.Collections.IEnumerable
        {
            var builder = (RuleBuilder<T, TCollection>)ruleBuilder;
            builder.AddValidator(new NotEmptyCollectionValidator<T, TCollection>());
            return builder;
        }

        /// <summary>
        /// Añade una regla que asegura que la propiedad no sea nula.
        /// </summary>
        public static IRuleBuilder<T, TProperty> NotNull<T, TProperty>(this IRuleBuilderInitial<T, TProperty> ruleBuilder)
            where TProperty : class // Aplica solo a tipos de referencia
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new NotNullValidator<T, TProperty>());
            return builder;
        }

        /// <summary>
        /// Añade una regla que asegura que la propiedad nullable no sea nula.
        /// </summary>
        public static IRuleBuilder<T, TProperty?> NotNull<T, TProperty>(this IRuleBuilderInitial<T, TProperty?> ruleBuilder)
           where TProperty : struct // Aplica solo a tipos de valor Nullable
        {
            var builder = (RuleBuilder<T, TProperty?>)ruleBuilder;
            builder.AddValidator(new NotNullValidator<T, TProperty?>());
            return builder;
        }

        /// <summary>
        /// Establece un mensaje personalizado para el último validador añadido.
        /// </summary>
        public static IRuleBuilder<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string message)
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.Rule.SetMessageForLastValidator(message);
            return builder;
        }

        // --- Implementaciones de Validadores Específicos ---

        // --- Helper para formatear mensajes de error --- (Se podría hacer más sofisticado con
        // localización, etc.)
        internal static string FormatErrorMessage<T>(PropertyRule<T, object> rule, string messageTemplate)
        {
            // Reemplaza placeholders comunes como {PropertyName}
            return messageTemplate.Replace("{PropertyName}", rule.PropertyName ?? "<Objeto>");
            // Añadir más reemplazos si es necesario ({PropertyValue}, etc.)
        }

        public class LessThanValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            public LessThanValidator(TProperty valueToCompare)
            {
                if (typeof(TProperty).IsValueType && Nullable.GetUnderlyingType(typeof(TProperty)) != null)
                    throw new ArgumentException($"El tipo parametro [{typeof(TProperty).Name}] debe ser tipo Valor y no nulo.", nameof(valueToCompare));
                ValueToCompare = valueToCompare;
            }

            // Propiedad pública para placeholder {ComparisonValue}
            public TProperty ValueToCompare { get; }

            // Mensaje por defecto que usa los placeholders internos
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => "'{PropertyName}' debe ser menor que {ComparisonValue}.";

            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                return Comparer.Default.Compare(value, ValueToCompare) < 0;
            }

        }

        internal class AllowedValuesValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            public AllowedValuesValidator(IRuleAllowedValue<TProperty> allowedValueDefinition)
            {
                AllowedValueDefinition = allowedValueDefinition;
            }

            // Propiedad pública para placeholder {AllowedValuesDescription}
            public IRuleAllowedValue<TProperty> AllowedValueDefinition { get; }

            // Mensaje por defecto que usa los placeholders internos
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) =>
                "El valor '{PropertyValue}' para '{PropertyName}' no es válido. Valores permitidos: {AllowedValuesDescription}.";

            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                if (value is null)
                    return true;  // Nulls son válidos a menos que se use NotNull

                return AllowedValueDefinition.IsAllowed(value);
            }

        }

        internal class GreaterThanValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            public GreaterThanValidator(TProperty valueToCompare)
            {
                if (typeof(TProperty).IsValueType && Nullable.GetUnderlyingType(typeof(TProperty)) != null)
                    throw new ArgumentException($"El tipo parametro [{typeof(TProperty).Name}] debe ser tipo Valor y no nulo.", nameof(valueToCompare));
                ValueToCompare = valueToCompare;
            }

            // Propiedad pública para placeholder {ComparisonValue}
            public TProperty ValueToCompare { get; }

            // Mensaje por defecto que usa los placeholders internos
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => "'{PropertyName}' debe ser mayor que {ComparisonValue}.";

            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                return Comparer.Default.Compare(value, ValueToCompare) > 0;
            }

        }

        internal class LengthValidator<T> : IPropertyValidator<T, string>
        {

            public LengthValidator(int min, int max)
            { Min = min; Max = max; }

            public int Max { get; }
            public int Min { get; }

            // Propiedad pública para placeholder {MinLength} Propiedad pública para placeholder {MaxLength}
            public string GetErrorMessage(ValidationContext<T> context, string value) => "La longitud de '{PropertyName}' debe estar entre {MinLength} y {MaxLength} caracteres.";

            public bool IsValid(ValidationContext<T> context, string value)
            {
                if (string.IsNullOrEmpty(value)) return true; // Longitud 0, si se requiere no vacío, usar NotEmpty
                return value.Length >= Min && value.Length <= Max;
            }

        }

        private sealed class InstancePredicateValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            private readonly string _errorMessageFormat;
            private readonly Func<T, TProperty, bool> _predicate;

            public InstancePredicateValidator(Func<T, TProperty, bool> predicate, string errorMessageFormat)
            {
                _predicate = predicate;
                _errorMessageFormat = errorMessageFormat;
            }

            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => _errorMessageFormat;

            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                // Pasa la instancia completa al predicado
                return _predicate(context.InstanceToValidate, value);
            }

        }

        private sealed class NotEmptyCollectionValidator<T, TCollection> : IPropertyValidator<T, TCollection> where TCollection : class, System.Collections.IEnumerable
        {
            public string GetErrorMessage(ValidationContext<T> context, TCollection value) => "La colección '{PropertyName}' no debe estar vacía.";

            public bool IsValid(ValidationContext<T> context, TCollection value)
            {
                if (value == null) return false; // Consider nulo como vacío para esta regla
                return value.GetEnumerator().MoveNext(); // Verifica si hay al menos un elemento
            }

        }

        private sealed class NotEmptyValidator<T> : IPropertyValidator<T, string>
        {

            public string GetErrorMessage(ValidationContext<T> context, string value) => "La propiedad '{PropertyName}' no debe estar vacía.";

            public bool IsValid(ValidationContext<T> context, string value) => !string.IsNullOrEmpty(value);

        }

        private sealed class NotNullValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => "La propiedad '{PropertyName}' no debe ser nula.";

            public bool IsValid(ValidationContext<T> context, TProperty value) => value is not null;

        }

        private sealed class PredicateValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {

            private readonly string _errorMessageFormat;
            private readonly Func<TProperty, bool> _predicate;

            public PredicateValidator(Func<TProperty, bool> predicate, string errorMessageFormat)
            {
                _predicate = predicate;
                _errorMessageFormat = errorMessageFormat;
            }

            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => _errorMessageFormat;

            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                // Si el valor es nulo, la condición generalmente no se aplica (a menos que el
                // predicado lo maneje) Reglas como NotNull deben usarse explícitamente si se
                // requiere que no sea nulo.
                if (value is null) return true;
                return _predicate(value);
            }

            // El formateo final con PropertyName se hace en PropertyRule
        }

        private sealed class RegularExpressionValidator<T> : IPropertyValidator<T, string>
        {

            private readonly Regex _regex;

            public RegularExpressionValidator(string pattern, RegexOptions options)
            {
                Pattern = pattern;
                _regex = new Regex(pattern, options);
            }

            public string Pattern { get; }

            public string GetErrorMessage(ValidationContext<T> context, string value) => "'{PropertyName}' no tiene el formato correcto ({Pattern}).";

            public bool IsValid(ValidationContext<T> context, string value)
            {
                if (string.IsNullOrEmpty(value)) return true; // No aplica a cadenas vacías
                return _regex.IsMatch(value);
            }

        }

        // Sobreescribir GetErrorMessage en PropertyRule para usar este formateo (Actualización:
        // Mejor hacerlo en el IPropertyValidator para tener el contexto completo)

        // Actualización en PropertyRule.Validate: Modificar la creación de ValidationFailure para
        // formatear el mensaje aquí usando el PropertyName y el mensaje base del IPropertyValidator.

        // Ejemplo de actualización en PropertyRule<T, TProperty>.Validate:
        /*
        public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
        {
            TProperty propertyValue = _propertyFunc(context.InstanceToValidate);
            var failures = new List<ValidationFailure>();

            foreach (var validator in _validators)
            {
                if (!validator.IsValid(context, propertyValue))
                {
                    string rawMessage = validator.GetErrorMessage(context, propertyValue);
                    string formattedMessage = rawMessage.Replace("{PropertyName}", PropertyName ?? "<Objeto>");
                    // Añadir más reemplazos si es necesario

                    failures.Add(new ValidationFailure(
                        PropertyName,
                        formattedMessage, // Usar mensaje formateado
                        propertyValue
                    ));
                    // CascadeMode logic...
                }
            }
            return failures;
        }
        */
        // Nota: He modificado directamente las implementaciones de GetErrorMessage en los validadores
        // para incluir {PropertyName} como placeholder, que será reemplazado al crear el
        // ValidationFailure. Esto simplifica un poco. El formateo final se hace al crear
        // ValidationFailure en PropertyRule.Validate.
        // **Revisando**: El código actual ya pasa el PropertyName al constructor de ValidationFailure.
        // El mensaje del validador DEBE incluir el placeholder '{PropertyName}' para que funcione.
        // Lo he añadido a los mensajes de ejemplo en los validadores.
    }
}