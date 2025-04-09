using System;
using System.Linq;

namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Resultado de una operación de validación.
    /// </summary>
    public class ValidationResult
    {
        #region Fields

        private readonly List<ValidationFailure> _errors = new List<ValidationFailure>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Lista de fallos de validación.
        /// </summary>
        public IReadOnlyList<ValidationFailure> Errors => _errors;

        /// <summary>
        /// Indica si la validación fue exitosa (no hay errores).
        /// </summary>
        public bool IsValid => !_errors.Any();

        #endregion Properties

        #region Methods

        /// <summary>
        /// Añade un fallo de validación a la lista.
        /// </summary>
        /// <param name="failure">El fallo a añadir.</param>
        public void AddFailure(ValidationFailure failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure));
            _errors.Add(failure);
        }

        /// <summary>
        /// Añade múltiples fallos de validación.
        /// </summary>
        /// <param name="failures">Los fallos a añadir.</param>
        public void AddFailures(IEnumerable<ValidationFailure> failures)
        {
            if (failures == null) throw new ArgumentNullException(nameof(failures));
            _errors.AddRange(failures.Where(f => f != null));
        }

        #endregion Methods
    }
}