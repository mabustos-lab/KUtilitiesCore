using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Helpers
{
    /// <summary>
    /// Excepción que se lanza cuando un objeto no cumple el contrato MVVM esperado
    /// (p. ej. no implementa <see cref="ISupportParentViewModel"/> o <see cref="IViewModelHelper"/>).
    /// Centralizarla en un tipo propio permite distinguir los errores de contrato
    /// de otros fallos de infraestructura.
    /// </summary>
    public class ViewModelSourceException : Exception
    {
        /// <summary>
        /// Inicializa una instancia sin mensaje; usado para escenarios de serialización
        /// y relleno diferido.
        /// </summary>
        public ViewModelSourceException()
        {
        }

        /// <summary>
        /// Inicializa la excepción con el mensaje que describe el contrato incumplido.
        /// </summary>
        /// <param name="message">Descripción del error de contrato.</param>
        public ViewModelSourceException(string message) : base(message)
        {
        }
    }
}
