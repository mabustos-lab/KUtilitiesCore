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

        private readonly List<string> _errorMessages = new List<string>();
        private readonly List<ValidationFailure> _errors = new List<ValidationFailure>();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Lista de mensajes generales
        /// </summary>
        public List<string> ErrorMessages => _errorMessages;

        /// <summary>
        /// Lista de fallos de validación.
        /// </summary>
        public IReadOnlyList<ValidationFailure> Errors => _errors;

        /// <summary>
        /// Indica si la validación fue exitosa (no hay errores).
        /// </summary>
        public bool IsValid => _errorMessages.Count == 0 && _errors.Count == 0;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Agrega un mensaje de error generico
        /// </summary>
        /// <param name="message"></param>
        public void AddErrorMessage(string message)
            => _errorMessages.Add(message);

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

        /// <summary>
        /// Borra todos los registros de error
        /// </summary>
        public void Clear()
        {
            _errorMessages.Clear();
            _errors.Clear();
        }

        #endregion Methods
    }
}