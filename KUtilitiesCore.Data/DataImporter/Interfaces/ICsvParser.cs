using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz para parseo de datos CSV
    /// </summary>
    public interface ICsvParser
    {
        /// <summary>
        /// Parsea un stream CSV a DataTable de forma síncrona
        /// </summary>
        /// <param name="stream">Stream con datos CSV</param>
        /// <param name="options">Opciones de parsing</param>
        /// <returns>DataTable con datos estructurados</returns>
        DataTable Parse(Stream stream, TextFileParsingOptions options);

        /// <summary>
        /// Parsea un stream CSV a DataTable de forma asíncrona
        /// </summary>
        /// <param name="stream">Stream con datos CSV</param>
        /// <param name="options">Opciones de parsing</param>
        /// <returns>Task que al completarse devuelve DataTable con datos estructurados</returns>
        Task<DataTable> ParseAsync(Stream stream, TextFileParsingOptions options);
    }
}
