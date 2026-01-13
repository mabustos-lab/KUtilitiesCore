using System;
using KUtilitiesCore.Data.ImportDefinition.Validation.Rules;

namespace KUtilitiesCore.Data.ImportDefinition.Validation
{
    /// <summary>
    /// Implementa el patrón Builder para facilitar la configuración fluida de reglas 
    /// sobre un <see cref="FieldDefinitionItem"/>.
    /// </summary>
    public class ImportRuleBuilder
    {
        private readonly IFieldDefinitionItem _fieldDefinition;

        public ImportRuleBuilder(IFieldDefinitionItem fieldDefinition)
        {
            _fieldDefinition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        }

        /// <summary>
        /// Agrega una regla personalizada a la definición del campo.
        /// </summary>
        public ImportRuleBuilder Rule(IImportValidationRule rule)
        {
            _fieldDefinition.ValidationRules.Add(rule);
            return this;
        }

        /// <summary>
        /// Valida que el valor no sea nulo ni vacío.
        /// </summary>
        public ImportRuleBuilder NotNull(string? errorMessage = null)
        {
            return Rule(new NotNullImportRule(errorMessage));
        }

        /// <summary>
        /// Valida que el valor cumpla con un predicado personalizado.
        /// </summary>
        /// <typeparam name="T">El tipo de dato esperado.</typeparam>
        /// <param name="predicate">La función de validación.</param>
        /// <param name="errorMessage">Mensaje de error si falla.</param>
        public ImportRuleBuilder Must<T>(Func<T, bool> predicate, string? errorMessage = "El valor no cumple con la regla de negocio.")
        {
            return Rule(new PredicateImportRule<T>(predicate, errorMessage));
        }

        /// <summary>
        /// Valida que el valor sea mayor que el límite especificado.
        /// </summary>
        public ImportRuleBuilder GreaterThan<T>(T value, string? errorMessage = null) where T : IComparable
        {
            return Rule(new ComparisonImportRule<T>(value, ComparisonImportRule<T>.ComparisonOperator.GreaterThan, errorMessage));
        }

        /// <summary>
        /// Valida que el valor sea menor que el límite especificado.
        /// </summary>
        public ImportRuleBuilder LessThan<T>(T value, string? errorMessage = null) where T : IComparable
        {
            return Rule(new ComparisonImportRule<T>(value, ComparisonImportRule<T>.ComparisonOperator.LessThan, errorMessage));
        }

        /// <summary>
        /// Valida que el valor sea mayor o igual que el límite especificado.
        /// </summary>
        public ImportRuleBuilder GreaterThanOrEqualTo<T>(T value, string? errorMessage = null) where T : IComparable
        {
            return Rule(new ComparisonImportRule<T>(value, ComparisonImportRule<T>.ComparisonOperator.GreaterThanOrEqual, errorMessage));
        }

        /// <summary>
        /// Valida que el valor sea menor o igual que el límite especificado.
        /// </summary>
        public ImportRuleBuilder LessThanOrEqualTo<T>(T value, string? errorMessage = null) where T : IComparable
        {
            return Rule(new ComparisonImportRule<T>(value, ComparisonImportRule<T>.ComparisonOperator.LessThanOrEqual, errorMessage));
        }
    }
}

