using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace KUtilitiesCore.Data.Validation
{
    // ----------------------------------------------------------------------
    // Métodos de extensión para el API Fluido (NotNull, NotEmpty, Must, etc.)
    // ----------------------------------------------------------------------
    public static class RuleBuilderExtensions
    {
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
        /// Añade una regla personalizada basada en un predicado.
        /// </summary>
        public static IRuleBuilder<T, TProperty> Must<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder,
            Func<TProperty, bool> predicate,
            string errorMessageFormat = "La propiedad '<%PropertyName%>' no cumple la condición.")
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
            string errorMessageFormat = "La propiedad '<%PropertyName%>' no cumple la condición basada en el objeto.")
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new InstancePredicateValidator<T, TProperty>(predicate, errorMessageFormat));
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
        /// Añade una regla que valida que un valor numérico sea menor que un valor específico.
        /// </summary>
        public static IRuleBuilder<T, TProperty> LessThan<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, TProperty valueToCompare)
            where TProperty : struct, IComparable<TProperty>
        {
            var builder = (RuleBuilder<T, TProperty>)ruleBuilder;
            builder.AddValidator(new LessThanValidator<T, TProperty>(valueToCompare));
            return builder;
        }

        // --- Implementaciones de Validadores Específicos ---

        private class NotNullValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {
            public bool IsValid(ValidationContext<T> context, TProperty value) => value != null;
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => "La propiedad '<%PropertyName%>' no debe ser nula.";
        }

        private class NotEmptyValidator<T> : IPropertyValidator<T, string>
        {
            public bool IsValid(ValidationContext<T> context, string value) => !string.IsNullOrEmpty(value);
            public string GetErrorMessage(ValidationContext<T> context, string value) => "La propiedad '<%PropertyName%>' no debe estar vacía.";
        }

        private class NotEmptyCollectionValidator<T, TCollection> : IPropertyValidator<T, TCollection> where TCollection : class, System.Collections.IEnumerable
        {
            public bool IsValid(ValidationContext<T> context, TCollection value)
            {
                if (value == null) return false; // Consider nulo como vacío para esta regla
                return value.GetEnumerator().MoveNext(); // Verifica si hay al menos un elemento
            }
            public string GetErrorMessage(ValidationContext<T> context, TCollection value) => "La colección '<%PropertyName%>' no debe estar vacía.";
        }

        private class PredicateValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {
            private readonly Func<TProperty, bool> _predicate;
            private readonly string _errorMessageFormat;
            public PredicateValidator(Func<TProperty, bool> predicate, string errorMessageFormat)
            {
                _predicate = predicate;
                _errorMessageFormat = errorMessageFormat;
            }
            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                // Si el valor es nulo, la condición generalmente no se aplica (a menos que el predicado lo maneje)
                // Reglas como NotNull deben usarse explícitamente si se requiere que no sea nulo.
                if (value == null) return true;
                return _predicate(value);
            }
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => _errorMessageFormat; // El formateo final con PropertyName se hace en PropertyRule
        }

        private class InstancePredicateValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {
            private readonly Func<T, TProperty, bool> _predicate;
            private readonly string _errorMessageFormat;
            public InstancePredicateValidator(Func<T, TProperty, bool> predicate, string errorMessageFormat)
            {
                _predicate = predicate;
                _errorMessageFormat = errorMessageFormat;
            }
            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                // Pasa la instancia completa al predicado
                return _predicate(context.InstanceToValidate, value);
            }
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => _errorMessageFormat;
        }


        private class LengthValidator<T> : IPropertyValidator<T, string>
        {
            private readonly int _min;
            private readonly int _max;
            public LengthValidator(int min, int max) { _min = min; _max = max; }
            public bool IsValid(ValidationContext<T> context, string value)
            {
                if (string.IsNullOrEmpty(value)) return true; // Longitud 0, si se requiere no vacío, usar NotEmpty
                return value.Length >= _min && value.Length <= _max;
            }
            public string GetErrorMessage(ValidationContext<T> context, string value) => $"La longitud de '<%PropertyName%>' debe estar entre {_min} y {_max} caracteres.";
        }

        private class AllowedValuesValidator<T, TProperty> : IPropertyValidator<T, TProperty>
        {
            private readonly IRuleAllowedValue<TProperty> _allowedValueDefinition;
            public AllowedValuesValidator(IRuleAllowedValue<TProperty> allowedValueDefinition)
            {
                _allowedValueDefinition = allowedValueDefinition;
            }
            public bool IsValid(ValidationContext<T> context, TProperty value)
            {
                // Si el valor es nulo, generalmente no se aplica esta regla.
                // Usar NotNull si es necesario.
                if (value == null) return true;
                return _allowedValueDefinition.IsAllowed(value);
            }
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) =>
                $"El valor '{value}' para '<%PropertyName%>' no es válido. Valores permitidos: {_allowedValueDefinition.GetAllowedDescription()}.";
        }

        private class RegularExpressionValidator<T> : IPropertyValidator<T, string>
        {
            private readonly Regex _regex;
            private readonly string _pattern;
            public RegularExpressionValidator(string pattern, RegexOptions options)
            {
                _pattern = pattern;
                _regex = new Regex(pattern, options);
            }
            public bool IsValid(ValidationContext<T> context, string value)
            {
                if (string.IsNullOrEmpty(value)) return true; // No aplica a cadenas vacías
                return _regex.IsMatch(value);
            }
            public string GetErrorMessage(ValidationContext<T> context, string value) => $"'<%PropertyName%>' no tiene el formato correcto ({_pattern}).";
        }

        private class GreaterThanValidator<T, TProperty> : IPropertyValidator<T, TProperty> where TProperty : struct, IComparable<TProperty>
        {
            private readonly TProperty _valueToCompare;
            public GreaterThanValidator(TProperty valueToCompare) { _valueToCompare = valueToCompare; }
            public bool IsValid(ValidationContext<T> context, TProperty value) => value.CompareTo(_valueToCompare) > 0;
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => $"'<%PropertyName%>' debe ser mayor que {_valueToCompare}.";
        }

        private class LessThanValidator<T, TProperty> : IPropertyValidator<T, TProperty> where TProperty : struct, IComparable<TProperty>
        {
            private readonly TProperty _valueToCompare;
            public LessThanValidator(TProperty valueToCompare) { _valueToCompare = valueToCompare; }
            public bool IsValid(ValidationContext<T> context, TProperty value) => value.CompareTo(_valueToCompare) < 0;
            public string GetErrorMessage(ValidationContext<T> context, TProperty value) => $"'<%PropertyName%>' debe ser menor que {_valueToCompare}.";
        }

        // --- Helper para formatear mensajes de error ---
        // (Se podría hacer más sofisticado con localización, etc.)
        internal static string FormatErrorMessage<T>(PropertyRule<T, object> rule, string messageTemplate)
        {
            // Reemplaza placeholders comunes como <%PropertyName%>
            return messageTemplate.Replace("<%PropertyName%>", rule.PropertyName ?? "<Objeto>");
            // Añadir más reemplazos si es necesario ({PropertyValue}, etc.)
        }

        // Sobreescribir GetErrorMessage en PropertyRule para usar este formateo
        // (Actualización: Mejor hacerlo en el IPropertyValidator para tener el contexto completo)

        // Actualización en PropertyRule.Validate:
        // Modificar la creación de ValidationFailure para formatear el mensaje aquí
        // usando el PropertyName y el mensaje base del IPropertyValidator.

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
                    string formattedMessage = rawMessage.Replace("<%PropertyName%>", PropertyName ?? "<Objeto>");
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
        // para incluir <%PropertyName%> como placeholder, que será reemplazado al crear el ValidationFailure.
        // Esto simplifica un poco. El formateo final se hace al crear ValidationFailure en PropertyRule.Validate.
        // **Revisando**: El código actual ya pasa el PropertyName al constructor de ValidationFailure.
        // El mensaje del validador DEBE incluir el placeholder '<%PropertyName%>' para que funcione.
        // Lo he añadido a los mensajes de ejemplo en los validadores.

    }
}
