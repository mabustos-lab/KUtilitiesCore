using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Fábrica para creación de lectores CSV con diferentes configuraciones
    /// </summary>
    public static class CsvSourceReaderFactory
    {
        /// <summary>
        /// Crea un lector CSV básico
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="separator">Separador de columnas (default: ",")</param>
        /// <returns>Instancia configurada de ICsvSourceReader</returns>
        public static ICsvSourceReader Create(string filePath, string separator = ",")
        {
            var options = new TextFileParsingOptions { Separator = separator };
            return new CsvSourceReader(filePath, null, null, options);
        }

        /// <summary>
        /// Crea un lector CSV con opciones específicas
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="options">Opciones de parsing</param>
        /// <returns>Instancia configurada de ICsvSourceReader</returns>
        public static ICsvSourceReader CreateWithOptions(string filePath, TextFileParsingOptions options)
        {
            return new CsvSourceReader(filePath, null, null, options);
        }

        /// <summary>
        /// Crea un lector CSV con encoding específico
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="encoding">Codificación del archivo</param>
        /// <param name="separator">Separador de columnas (default: ",")</param>
        /// <returns>Instancia configurada de ICsvSourceReader</returns>
        public static ICsvSourceReader CreateWithEncoding(string filePath, Encoding encoding, string separator = ",")
        {
            var options = new TextFileParsingOptions
            {
                Separator = separator,
                Encoding = encoding
            };
            return new CsvSourceReader(filePath, null, null, options);
        }

        /// <summary>
        /// Crea un lector CSV para tabuladores (TSV)
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <returns>Instancia configurada para TSV</returns>
        public static ICsvSourceReader CreateTsvReader(string filePath)
        {
            var options = new TextFileParsingOptions { Separator = "\t" };
            return new CsvSourceReader(filePath, null, null, options);
        }

        /// <summary>
        /// Crea un lector CSV sin encabezado
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <param name="separator">Separador de columnas (default: ",")</param>
        /// <returns>Instancia configurada sin encabezado</returns>
        public static ICsvSourceReader CreateWithoutHeader(string filePath, string separator = ",")
        {
            var options = new TextFileParsingOptions
            {
                Separator = separator,
                HasHeader = false
            };
            return new CsvSourceReader(filePath, null, null, options);
        }
    }
}
