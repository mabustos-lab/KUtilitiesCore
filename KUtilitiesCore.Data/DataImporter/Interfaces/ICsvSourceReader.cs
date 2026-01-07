using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz para obtener datos de una fuente de texto plano
    /// </summary>
    public interface ICsvSourceReader:IDataSourceReader
    {        
        /// <summary>
        /// Indica el archivo fuente
        /// </summary>
        string FilePath { get; set; }
        /// <summary>
        /// Indica el caracter de separación de cada columna
        /// </summary>
        string SpliterChar { get; set; }
    }
}
