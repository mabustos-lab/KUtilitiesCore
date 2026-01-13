using System;
using System.Collections.Generic;
using System.Linq;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Extensions;

namespace KUtilitiesCore.Data.ImportDefinition.Validation.Rules
{
    /// <summary>
    /// Regla base abstracta para reducir código repetitivo en implementaciones concretas.
    /// </summary>
    public abstract class ImportValidationRuleBase : IImportValidationRule
    {
        /// <inheritdoc/>
        protected string? ErrorMessage { get; set; }

        protected ImportValidationRuleBase(string? errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        /// <inheritdoc/>
        public abstract IEnumerable<ValidationFailure> Validate(object value, string fieldName);
        /// <inheritdoc/>
        protected ValidationFailure CreateFailure(string fieldName, string defaultMessage,int idxRow=-1, object? value = null)
        {
            return new ValidationFailure(fieldName, ErrorMessage ?? defaultMessage, idxRow, value);
        }
    }
}