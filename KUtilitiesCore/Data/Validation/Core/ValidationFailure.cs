using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Representa un fallo de validación individual.
    /// </summary>
    public class ValidationFailure(string propertyName, string errorMessage, object? attemptedValue = null)
    {
        #region Properties

        /// <summary>
        /// Valor que causó el fallo (opcional).
        /// </summary>
        public object? AttemptedValue { get; } = attemptedValue;

        /// <summary>
        /// Mensaje de error descriptivo.
        /// </summary>
        public string ErrorMessage { get; } = errorMessage;

        /// <summary>
        /// Nombre de la propiedad que falló la validación. Puede ser nulo para validaciones a nivel
        /// de objeto.
        /// </summary>
        public string PropertyName { get; } = propertyName;

        #endregion Properties

        #region Methods

        public override string ToString() => ErrorMessage;

        #endregion Methods
    }
}