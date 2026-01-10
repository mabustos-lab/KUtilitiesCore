using System;
using System.Linq;
using System.Text;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz para fuentes de datos basadas en texto
    /// </summary>
    public interface ITextSourceReader : IDataSourceReader
    {
        /// <summary>
        /// Codificación del archivo de texto
        /// </summary>
        /// <value>Encoding utilizado para leer el archivo</value>
        Encoding Encoding { get; set; }
    }
}
