using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Permite ejecutar una acción almacenada débilmente con un parámetro de tipo object.
    /// </summary>
    public interface IExecuteWithObject
    {
        /// <summary>
        /// Ejecuta la acción con el parámetro proporcionado.
        /// </summary>
        /// <param name="parameter">El parámetro para la acción, que será casteado al tipo apropiado.</param>
        void ExecuteWithObject(object? parameter);
    }
}
