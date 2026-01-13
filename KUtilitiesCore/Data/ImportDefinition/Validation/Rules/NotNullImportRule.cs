using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.ImportDefinition.Validation.Rules
{
    /// <summary>
    /// Regla para validar que un valor no sea nulo.
    /// </summary>
    public class NotNullImportRule : ImportValidationRuleBase
    {
        public NotNullImportRule(string? errorMessage = null) : base(errorMessage) { }
        /// <inheritdoc/>
        public override IEnumerable<ValidationFailure> Validate(object value, string fieldName)
        {
            if (value == null || value is string s && string.IsNullOrWhiteSpace(s))
            {
                yield return CreateFailure(fieldName, $"El campo '{fieldName}' es requerido.", -1, value);
            }
        }
    }
}