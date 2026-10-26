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

        /// <summary>
        /// Inicializa la regla con un mensaje de error opcional.
        /// </summary>
        /// <param name="errorMessage">Mensaje de error de la regla; si es <c>null</c>, cada regla
        /// usa su mensaje por defecto al crear el fallo.</param>
        protected ImportValidationRuleBase(string? errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        /// <inheritdoc/>
        public abstract IEnumerable<ValidationFailure> Validate(object? value, string fieldName);
        /// <inheritdoc/>
        protected ValidationFailure CreateFailure(string fieldName, string? defaultMessage, int idxRow = -1, object? value = null)
        {
            // El respaldo final con string.Empty es defensivo: en la práctica siempre hay
            // un mensaje (propiedad o default) y un fallo sin mensaje no aporta información.
            return new ValidationFailure(fieldName, ErrorMessage ?? defaultMessage ?? string.Empty, idxRow, value);
        }
    }
}