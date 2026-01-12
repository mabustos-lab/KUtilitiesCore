using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz abstracta para lectura de libros de Excel
    /// </summary>
    public interface IExcelWorkbookReader : IDisposable
    {
        /// <summary>
        /// Obtiene la lista de nombres de hojas del libro
        /// </summary>
        /// <returns>Lista de nombres de hojas</returns>
        IReadOnlyList<string> GetSheetNames();

        /// <summary>
        /// Obtiene un lector para una hoja específica
        /// </summary>
        /// <param name="sheetName">Nombre de la hoja</param>
        /// <returns>Lector de hoja</returns>
        IExcelWorksheetReader GetWorksheet(string sheetName);

        /// <summary>
        /// Obtiene un lector para la primera hoja
        /// </summary>
        /// <returns>Lector de hoja</returns>
        IExcelWorksheetReader GetFirstWorksheet();

        /// <summary>
        /// Verifica si el libro contiene una hoja con el nombre especificado
        /// </summary>
        /// <param name="sheetName">Nombre de la hoja</param>
        /// <returns>True si la hoja existe</returns>
        bool ContainsSheet(string sheetName);
    }

    /// <summary>
    /// Interfaz abstracta para lectura de hojas de Excel
    /// </summary>
    public interface IExcelWorksheetReader
    {
        /// <summary>
        /// Nombre de la hoja
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Lee todas las filas con datos
        /// </summary>
        /// <returns>Enumeración de filas</returns>
        IEnumerable<IExcelRow> ReadRows();

        /// <summary>
        /// Lee las filas dentro de un rango específico
        /// </summary>
        /// <param name="startRow">Fila inicial (1-based)</param>
        /// <param name="endRow">Fila final (1-based)</param>
        /// <returns>Enumeración de filas</returns>
        IEnumerable<IExcelRow> ReadRows(int startRow, int endRow);

        /// <summary>
        /// Obtiene el número total de filas usadas
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Obtiene el número total de columnas usadas
        /// </summary>
        int ColumnCount { get; }
    }

    /// <summary>
    /// Interfaz abstracta para representar una fila de Excel
    /// </summary>
    public interface IExcelRow
    {
        /// <summary>
        /// Número de fila (1-based)
        /// </summary>
        int RowNumber { get; }

        /// <summary>
        /// Obtiene las celdas de esta fila
        /// </summary>
        IEnumerable<IExcelCell> Cells { get; }

        /// <summary>
        /// Indica si la fila está completamente vacía
        /// </summary>
        bool IsEmpty { get; }
    }

    /// <summary>
    /// Interfaz abstracta para representar una celda de Excel
    /// </summary>
    public interface IExcelCell
    {
        /// <summary>
        /// Dirección de la celda (ej: "A1")
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Número de columna (1-based)
        /// </summary>
        int ColumnNumber { get; }

        /// <summary>
        /// Valor sin procesar de la celda
        /// </summary>
        object RawValue { get; }

        /// <summary>
        /// Tipo de datos de la celda
        /// </summary>
        string DataType { get; }

        /// <summary>
        /// Valor formateado como string
        /// </summary>
        string FormattedValue { get; }

        /// <summary>
        /// Indica si la celda está vacía
        /// </summary>
        bool IsEmpty { get; }
    }

    /// <summary>
    /// Interfaz para conversión de valores de celda
    /// </summary>
    public interface ICellValueConverter
    {
        /// <summary>
        /// Convierte el valor de una celda a string
        /// </summary>
        /// <param name="cell">Celda a convertir</param>
        /// <returns>Valor convertido a string</returns>
        string ConvertToString(IExcelCell cell);
    }
}
