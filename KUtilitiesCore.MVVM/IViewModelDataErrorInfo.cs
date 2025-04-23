using System;
using System.ComponentModel;
using System.Linq;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Proporciona funcionalidad para gestionar información de error personalizada
    /// que puede ser enlazada a una interfaz de usuario.
    /// </summary>
    public interface IViewModelDataErrorInfo : IDataErrorInfo
    {
        /// <summary>
        /// Obtiene un valor que indica si existen errores de validación.
        /// </summary>
        bool HasValidationErrors { get; }

        /// <summary>
        /// Elimina todos los mensajes de error registrados.
        /// </summary>
        void ClearErrors();

        /// <summary>
        /// Establece un mensaje de error general para el objeto.
        /// </summary>
        /// <param name="errorMessage">El mensaje de error a establecer.</param>
        void SetError(string errorMessage);

        /// <summary>
        /// Establece un mensaje de error para una propiedad específica del objeto.
        /// </summary>
        /// <param name="propertyName">El nombre de la propiedad con el error.</param>
        /// <param name="errorMessage">El mensaje de error a establecer.</param>
        void SetError(string propertyName, string errorMessage);
    }
}
