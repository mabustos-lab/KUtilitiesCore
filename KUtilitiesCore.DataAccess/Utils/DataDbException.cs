using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Utils
{
    /// <summary>
    /// Encapsula la Excepción indepedente de la base de datos usada
    /// </summary>
    public class DataDbException : Exception
    {
        private readonly string _ErrorCaption;
        private readonly string _ErrorMessage;

        /// <summary>
        /// Inicializa una nueva instancia de la clase DbException.
        /// </summary>
        /// <param name="errorMessage">
        /// Texto de un mensaje de error.
        /// </param>
        /// <param name="errorCaption">
        /// Texto de título del mensaje de error.
        /// </param>
        /// <param name="innerException">
        /// Una excepción subyacente.
        /// </param>
        public DataDbException(string errorMessage, string errorCaption, Exception innerException) : base(innerException.Message, innerException)
        {
            this._ErrorMessage = errorMessage;
            this._ErrorCaption = errorCaption;
        }

        public DataDbException(string errorMessage, string errorCaption) : base()
        {
            this._ErrorMessage = errorMessage;
            this._ErrorCaption = errorCaption;
        }

        /// <summary>
        /// El texto del título del mensaje de error.
        /// </summary>
        public string ErrorCaption
        {
            get
            {
                return _ErrorCaption;
            }
        }

        /// <summary>
        /// El texto del mensaje de error.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
        }
    }
}
