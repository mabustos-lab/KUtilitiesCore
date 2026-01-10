using ClosedXML.Excel;
using KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml;
using KUtilitiesCore.Data.DataImporter.Infrastructure;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Implementación refactorizada de lector de Excel
    /// </summary>
    public class ExcelSourceReader : IExcelSourceReader
    {
        private readonly IExcelWorkbookReaderFactory _workbookFactory;
        private readonly IDiskFileReader _fileReader;
        private readonly ICellValueConverter _cellConverter;
        private readonly ExcelParsingOptions _options;

        private IExcelWorkbookReader _workbookReader;
        private bool _disposed;

        /// <inheritdoc/>
        public string FilePath { get; set; }
        /// <inheritdoc/>
        public string SheetName { get; set; }
        /// <inheritdoc/>
        public bool HasHeader { get; set; } = true;
        /// <inheritdoc/>
        public bool CanRead => !string.IsNullOrEmpty(FilePath) &&
                              _fileReader.FileExists(FilePath) &&
                              !string.IsNullOrEmpty(SheetName);

        /// <summary>
        /// Constructor principal con inyección de dependencias
        /// </summary>
        public ExcelSourceReader(
            string filePath,
            IExcelWorkbookReaderFactory workbookFactory = null,
            IDiskFileReader fileReader = null,
            ICellValueConverter cellConverter = null,
            ExcelParsingOptions options = null)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _workbookFactory = workbookFactory ?? CreateDefaultFactory();
            _fileReader = fileReader ?? new DefaultDiskFileReader();
            _options = options?.Clone() ?? new ExcelParsingOptions();
            _cellConverter = cellConverter ?? new DefaultCellValueConverter(_options);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSheets()
        {
            EnsureWorkbookOpen();
            return _workbookReader.GetSheetNames();
        }
        /// <inheritdoc/>
        public IReadOnlyList<SheetInfo> GetSheetInfo()
        {
            EnsureWorkbookOpen();
            var sheets = new List<SheetInfo>();
            var sheetNames = _workbookReader.GetSheetNames();

            for (int i = 0; i < sheetNames.Count; i++)
            {
                var worksheet = _workbookReader.GetWorksheet(sheetNames[i]);
                sheets.Add(new SheetInfo
                {
                    Name = worksheet.Name,
                    RowCount = worksheet.RowCount,
                    ColumnCount = worksheet.ColumnCount,
                    Position = i
                });
            }

            return sheets.AsReadOnly();
        }
        /// <inheritdoc/>
        public DataTable ReadData()
        {
            ValidatePreconditions();
            EnsureWorkbookOpen();

            IExcelWorksheetReader worksheetReader = GetWorksheetReader();
            DataTable dataTable = new DataTable(worksheetReader.Name);

            var rows = ReadWorksheetRows(worksheetReader);
            ProcessRowsToDataTable(rows, dataTable);

            dataTable.AcceptChanges();
            return dataTable;
        }
        /// <inheritdoc/>
        public async Task<DataTable> ReadDataAsync()
        {
            return await Task.Run(() => ReadData())
                .ConfigureAwait(false);
        }

        private void ValidatePreconditions()
        {
            if (string.IsNullOrEmpty(FilePath))
                throw new InvalidOperationException("La ruta del archivo no está configurada");

            if (!_fileReader.FileExists(FilePath))
                throw new System.IO.FileNotFoundException($"El archivo no existe: {FilePath}", FilePath);
        }

        private void EnsureWorkbookOpen()
        {
            if (_workbookReader != null)
                return;

            using var stream = _fileReader.OpenRead(FilePath);
            _workbookReader = _workbookFactory.CreateFromStream(stream, leaveOpen: false);
        }

        private IExcelWorksheetReader GetWorksheetReader()
        {
            if (!string.IsNullOrEmpty(SheetName))
            {
                if (!_workbookReader.ContainsSheet(SheetName))
                {
                    if (_options.ThrowOnMissingSheet)
                        throw new ArgumentException($"La hoja '{SheetName}' no existe en el archivo");

                    // Si no existe y no se debe lanzar excepción, usar la primera hoja
                    return _workbookReader.GetFirstWorksheet();
                }
                return _workbookReader.GetWorksheet(SheetName);
            }

            // Si no se especificó hoja, usar la primera
            return _workbookReader.GetFirstWorksheet();
        }

        private IEnumerable<IExcelRow> ReadWorksheetRows(IExcelWorksheetReader worksheet)
        {
            if (_options.EndRow.HasValue)
            {
                return worksheet.ReadRows(_options.StartRow, _options.EndRow.Value);
            }

            return worksheet.ReadRows();
        }

        private void ProcessRowsToDataTable(IEnumerable<IExcelRow> rows, DataTable dataTable)
        {
            var rowsEnumerator = rows.GetEnumerator();
            List<string> headers = new List<string>();

            // Procesar encabezados si corresponde
            if (_options.HasHeader && rowsEnumerator.MoveNext())
            {
                headers = ProcessHeaderRow(rowsEnumerator.Current, dataTable);
            }
            else if (!_options.HasHeader)
            {
                // Generar encabezados automáticos basados en la primera fila
                if (rowsEnumerator.MoveNext())
                {
                    headers = GenerateAutoHeaders(rowsEnumerator.Current, dataTable);
                    // Reiniciar enumerador para incluir la primera fila como datos
                    rowsEnumerator = rows.GetEnumerator();
                    rowsEnumerator.MoveNext();
                }
            }

            // Procesar filas de datos
            while (rowsEnumerator.MoveNext())
            {
                var row = rowsEnumerator.Current;

                if (_options.IgnoreEmptyRows && row.IsEmpty)
                    continue;

                ProcessDataRow(row, headers, dataTable);
            }
        }

        private List<string> ProcessHeaderRow(IExcelRow headerRow, DataTable dataTable)
        {
            var headers = new List<string>();
            int columnIndex = 1;

            foreach (var cell in headerRow.Cells)
            {
                string headerName = _cellConverter.ConvertToString(cell);

                if (string.IsNullOrEmpty(headerName))
                    headerName = $"Column{columnIndex}";

                // Evitar duplicados
                string uniqueName = headerName;
                int suffix = 1;
                while (headers.Contains(uniqueName))
                {
                    uniqueName = $"{headerName}_{suffix}";
                    suffix++;
                }

                headers.Add(uniqueName);
                dataTable.Columns.Add(uniqueName, typeof(string));
                columnIndex++;
            }

            return headers;
        }
        /// <inheritdoc/>
        private static List<string> GenerateAutoHeaders(IExcelRow firstRow, DataTable dataTable)
        {
            var headers = new List<string>();
            int columnCount = 0;

            foreach (var cell in firstRow.Cells)
            {
                columnCount++;
                string headerName = $"Column{columnCount}";
                headers.Add(headerName);
                dataTable.Columns.Add(headerName, typeof(string));
            }

            return headers;
        }

        private void ProcessDataRow(IExcelRow excelRow, List<string> headers, DataTable dataTable)
        {
            DataRow dataRow = dataTable.NewRow();
            int columnIndex = 0;

            foreach (var cell in excelRow.Cells)
            {
                if (columnIndex >= headers.Count)
                    break;

                string value = _cellConverter.ConvertToString(cell);
                dataRow[columnIndex] = string.IsNullOrEmpty(value) ? DBNull.Value : value;
                columnIndex++;
            }

            // Rellenar columnas faltantes con valores vacíos
            for (int i = columnIndex; i < headers.Count; i++)
            {
                dataRow[i] = DBNull.Value;
            }

            dataTable.Rows.Add(dataRow);
        }

        private IExcelWorkbookReaderFactory CreateDefaultFactory()
        {
            // Por defecto, usar ClosedXML para .xlsx/.xlsm
            // Se podría extender para detectar y usar diferentes factories según el formato
            return new ClosedXmlWorkbookReaderFactory();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _workbookReader?.Dispose();
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Implementación por defecto de ICellValueConverter
    /// </summary>
    internal class DefaultCellValueConverter : ICellValueConverter
    {
        private readonly ExcelParsingOptions _options;

        public DefaultCellValueConverter(ExcelParsingOptions options)
        {
            _options = options;
        }

        public string ConvertToString(IExcelCell cell)
        {
            if (cell == null || cell.IsEmpty)
                return _options.TreatEmptyAsNull ? null : string.Empty;

            string value = cell.FormattedValue;

            if (_options.TrimValues && value != null)
            {
                value = value.Trim();
            }

            // Formatear fechas si se especificó formato
            if (!string.IsNullOrEmpty(_options.DateFormat) &&
                DateTime.TryParse(value, out DateTime dateValue))
            {
                value = dateValue.ToString(_options.DateFormat);
            }

            return value;
        }
    }
}
