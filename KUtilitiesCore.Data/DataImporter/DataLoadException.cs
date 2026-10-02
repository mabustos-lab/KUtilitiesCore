using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Excepción personalizada para errores de carga de datos
    /// </summary>
    public class DataLoadException : Exception
    {
        /// <summary>
        /// Inicializa una nueva instancia sin mensaje ni excepción interna.
        /// </summary>
        public DataLoadException() { }

        /// <summary>
        /// Inicializa una nueva instancia con el mensaje que describe el error de carga.
        /// </summary>
        /// <param name="message">Mensaje que describe el motivo de la excepción.</param>
        public DataLoadException(string message) : base(message) { }

        /// <summary>
        /// Inicializa una nueva instancia con un mensaje y la excepción interna que originó el error de carga.
        /// </summary>
        /// <param name="message">Mensaje que describe el motivo de la excepción.</param>
        /// <param name="inner">Excepción interna que originó esta excepción.</param>
        public DataLoadException(string message, Exception inner) : base(message, inner) { }
    }
}
