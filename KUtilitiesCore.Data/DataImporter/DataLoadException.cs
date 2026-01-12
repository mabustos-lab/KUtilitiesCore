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
        public DataLoadException() { }
        public DataLoadException(string message) : base(message) { }
        public DataLoadException(string message, Exception inner) : base(message, inner) { }
    }
}
