using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Fábrica para creación de lectores de Excel
    /// </summary>
    public static class ExcelSourceReaderFactory
    {
        /// <summary>
        /// Crea un lector de Excel básico
        /// </summary>
        public static IExcelSourceReader Create(string filePath, string sheetName = null)
        {
            var options = new ExcelParsingOptions();
            if (!string.IsNullOrEmpty(sheetName))
            {
                options.SheetName = sheetName;
            }

            return new ExcelSourceReader(filePath, null, null, null, options);
        }
        /// <summary>
        /// Obtiene la colección de nombre de las hojas de un libro de Excel
        /// </summary>
        public static IReadOnlyList<string> GetSheets(string filePath)
        {
            var options = new ExcelParsingOptions();
            using var xlxs= new ExcelSourceReader(filePath, null, null, null, options);
            return xlxs.GetSheets();
        }
        /// <summary>
        /// Crea un lector de Excel con opciones específicas
        /// </summary>
        public static IExcelSourceReader CreateWithOptions(string filePath,
            ExcelParsingOptions options, IExcelWorkbookReaderFactory workbookFactory = null,
            IDiskFileReader diskFileReader=null, ICellValueConverter cellValueConverter = null)
        {
            return new ExcelSourceReader(filePath, workbookFactory, diskFileReader, cellValueConverter, options);
        }

        /// <summary>
        /// Crea un lector de Excel sin encabezado
        /// </summary>
        public static IExcelSourceReader CreateWithoutHeader(string filePath, string sheetName = null)
        {
            var options = new ExcelParsingOptions
            {
                HasHeader = false,
                SheetName = sheetName
            };

            return new ExcelSourceReader(filePath, null, null, null, options);
        }

        /// <summary>
        /// Crea un lector de Excel para un rango específico
        /// </summary>
        public static IExcelSourceReader CreateWithRange(
            string filePath,
            string sheetName,
            int startRow,
            int? endRow = null)
        {
            var options = new ExcelParsingOptions
            {
                SheetName = sheetName,
                StartRow = startRow,
                EndRow = endRow
            };

            return new ExcelSourceReader(filePath, null, null, null, options);
        }
    }
}
