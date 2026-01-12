using System;
using System.Linq;

namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Contenedor de los resultados de una validación. Soporta tanto errores detallados de propiedades como errores
    /// genéricos.
    /// </summary>
    [Serializable]
    public class ValidationResult
    {
        private readonly List<ValidationFailureBase> _errors;

        /// <summary>
        /// Colección polimórfica de errores. Puede contener tanto ValidationFailure como GenericFailure.
        /// </summary>
        public virtual IList<ValidationFailureBase> Errors => _errors;

        /// <summary>
        /// Indica si la validación fue exitosa (sin errores).
        /// </summary>
        public virtual bool IsValid => _errors.Count == 0;

        public ValidationResult() { _errors = new List<ValidationFailureBase>(); }

        public ValidationResult(IEnumerable<ValidationFailureBase> failures)
        { _errors = failures.Where(failure => failure != null).ToList(); }

        /// <summary>
        /// Agrega un error detallado de propiedad.
        /// </summary>
        public void AddError(ValidationFailure failure) { _errors.Add(failure); }

        /// <summary>
        /// Agrega un error detallado de propiedad (Helper).
        /// </summary>
        public void AddError(string propertyName, string errorMessage, int indexRow = -1, object? attemptedValue = null)
        { _errors.Add(new ValidationFailure(propertyName, errorMessage, indexRow, attemptedValue)); }

        /// <summary>
        /// Agrega un mensaje de error genérico (sin propiedad asociada).
        /// </summary>
        public void AddErrorMessage(string errorMessage) { _errors.Add(new GenericFailure(errorMessage)); }

        /// <summary>
        /// Obtiene solo los mensajes de error (tanto genéricos como de propiedad) como una lista de strings. Útil para
        /// mostrar resúmenes simples en UI.
        /// </summary>
        public IEnumerable<string> GetErrorMessages() { return _errors.Select(x => x.ErrorMessage); }
    }
}