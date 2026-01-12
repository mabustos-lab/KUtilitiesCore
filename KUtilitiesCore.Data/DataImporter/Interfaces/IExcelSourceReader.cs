using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz refactorizada para lectores de Excel
    /// </summary>
    public interface IExcelSourceReader : IDataSourceReader, IDisposable
    {
        /// <summary>
        /// Ruta del archivo Excel
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Nombre de la hoja a procesar
        /// </summary>
        string SheetName { get; set; }

        /// <summary>
        /// Indica si la primera fila contiene encabezados
        /// </summary>
        bool HasHeader { get; set; }

        /// <summary>
        /// Obtiene la lista de nombres de todas las hojas disponibles
        /// </summary>
        /// <returns>Lista de nombres de hojas</returns>
        IReadOnlyList<string> GetSheets();

        /// <summary>
        /// Obtiene información adicional sobre las hojas
        /// </summary>
        /// <returns>Información detallada de hojas</returns>
        IReadOnlyList<SheetInfo> GetSheetInfo();
    }

    /// <summary>
    /// Información detallada de una hoja de Excel
    /// </summary>
    public class SheetInfo
    {
        /// <summary>
        /// Nombre de la hoja
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Número de filas con datos
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Número de columnas con datos
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Posición de la hoja (índice base 0)
        /// </summary>
        public int Position { get; set; }
    }
}
