using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Validation.Core
{
    /// <summary>
    /// Representa un error de validación general que no está atado a una propiedad específica ni a
    /// un valor intentado. Es una versión ligera de un fallo.
    /// </summary>
    [Serializable]
    public class GenericFailure : ValidationFailureBase
    {
        #region Constructors

        public GenericFailure(string errorMessage) : base(errorMessage)
        {
        }

        #endregion Constructors
    }

    /// <summary>
    /// Representa el fallo de validación de una propiedad específica. Contiene metadatos detallados
    /// sobre qué falló y por qué.
    /// </summary>
    [Serializable]
    public class ValidationFailure : ValidationFailureBase
    {
        #region Constructors

        public ValidationFailure(string propertyName, string error, int idxRow, object? attemptedValue = null)
                    : base(error)
        {
            PropertyName = propertyName;
            AttemptedValue = attemptedValue ?? string.Empty;
            IndexRow = idxRow;
        }

        /// <summary>
        /// Crea una nueva instancia de ValidationFailure.
        /// </summary>
        public ValidationFailure(string propertyName, string error) : this(propertyName, error, -1, null)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// El valor que se intentó asignar y causó el fallo.
        /// </summary>
        public object AttemptedValue { get; set; }

        /// <summary>
        /// Estado o severidad personalizada (opcional).
        /// </summary>
        public object CustomState { get; set; }

        /// <summary>
        /// Código de error personalizado (opcional).
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Establece el indice dde la fila del error.
        /// </summary>
        public int IndexRow { get; set; }

        /// <summary>
        /// El nombre de la propiedad que falló la validación.
        /// </summary>
        public string PropertyName { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"Propiedad: {PropertyName ?? "<Objeto>"}{(IndexRow>=0?$"[Index: {IndexRow}]:" :"")} Error: {ErrorMessage} Valor: '{(AttemptedValue??"<null>")}'";
        }

        #endregion Methods
    }

    /// <summary>
    /// Clase base abstracta para cualquier tipo de fallo de validación. Permite tener una lista
    /// polimórfica en ValidationResult.
    /// </summary>
    [Serializable]
    public abstract class ValidationFailureBase
    {
        #region Constructors

        protected ValidationFailureBase(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// El mensaje de error.
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return ErrorMessage;
        }

        #endregion Methods
    }
}