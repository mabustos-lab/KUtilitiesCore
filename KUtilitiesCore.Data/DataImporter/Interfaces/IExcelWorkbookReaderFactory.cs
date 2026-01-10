using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Fábrica abstracta para crear lectores de libros de Excel
    /// </summary>
    public interface IExcelWorkbookReaderFactory
    {
        /// <summary>
        /// Crea un lector desde un archivo
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <returns>Lector de libro</returns>
        IExcelWorkbookReader CreateFromFile(string filePath);

        /// <summary>
        /// Crea un lector desde un stream
        /// </summary>
        /// <param name="stream">Stream con datos Excel</param>
        /// <param name="leaveOpen">Indica si dejar el stream abierto</param>
        /// <returns>Lector de libro</returns>
        IExcelWorkbookReader CreateFromStream(Stream stream, bool leaveOpen = false);

        /// <summary>
        /// Verifica si el factory soporta un archivo específico
        /// </summary>
        /// <param name="filePath">Ruta del archivo</param>
        /// <returns>True si soporta el formato</returns>
        bool SupportsFile(string filePath);
    }
}
