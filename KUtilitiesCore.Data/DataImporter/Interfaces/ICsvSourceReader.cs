using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz específica para fuentes de datos CSV
    /// </summary>
    /// <remarks>
    public interface ICsvSourceReader : ITextSourceReader
    {
        /// <summary>
        /// Ruta completa del archivo CSV
        /// </summary>
        /// <value>Path del archivo a leer</value>
        string FilePath { get; set; }

        /// <summary>
        /// Carácter separador de columnas
        /// </summary>
        /// <value>String que actúa como separador (default: ",")</value>
        string SpliterChar { get; set; }
    }
}
