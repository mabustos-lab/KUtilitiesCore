using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz para obtener los datos de un origen de datos
    /// </summary>
    public interface IDataSourceReader
    {
        /// <summary>
        /// Indica si cunple los requisitos para extraer datos del origen.
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        /// Lee la fuente y devuelve un DataTable crudo.
        /// </summary>
        DataTable ReadData();
    }
}
