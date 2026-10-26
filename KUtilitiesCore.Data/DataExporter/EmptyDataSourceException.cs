using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataExporter
{
    /// <summary>
    /// Excepción que se lanza cuando el origen de datos a exportar no contiene filas.
    /// Permite distinguir este caso de uso de otros errores de exportación.
    /// </summary>
    [Serializable]
    public class EmptyDataSourceException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia sin mensaje ni excepción interna.
        /// </summary>
        public EmptyDataSourceException() { }

        /// <summary>
        /// Inicializa una nueva instancia con el mensaje que describe el error.
        /// </summary>
        /// <param name="message">Mensaje que describe el motivo de la excepción.</param>
        public EmptyDataSourceException(string message) : base(message) { }

        /// <summary>
        /// Inicializa una nueva instancia con un mensaje y la excepción interna que la originó.
        /// </summary>
        /// <param name="message">Mensaje que describe el motivo de la excepción.</param>
        /// <param name="innerException">Excepción interna que originó esta excepción.</param>
        public EmptyDataSourceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
