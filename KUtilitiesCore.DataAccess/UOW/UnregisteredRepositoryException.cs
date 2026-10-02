using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Excepción que se lanza cuando se solicita al Unit of Work un repositorio
    /// que no fue registrado previamente, de modo que el fallo de configuración
    /// sea explícito y detectable en lugar de un error críptico de DI.
    /// </summary>
    public class UnregisteredRepositoryException : Exception
    {
        /// <summary>
        /// Inicializa la excepción con el mensaje que describe el repositorio faltante.
        /// </summary>
        /// <param name="message">Descripción del error de registro.</param>
        public UnregisteredRepositoryException(string message) : base(message) { }
        /// <summary>
        /// Inicializa la excepción con un mensaje y la causa interna que originó el fallo.
        /// </summary>
        /// <param name="message">Descripción del error de registro.</param>
        /// <param name="innerException">Excepción interna que causó el error.</param>
        public UnregisteredRepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
