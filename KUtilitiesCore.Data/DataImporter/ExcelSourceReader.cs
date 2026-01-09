using ClosedXML.Excel;
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
    /// Implementación de lector de datos desde archivos Excel (.xlsx) usando ClosedXML.
    /// </summary>
    public class ExcelSourceReader : IExcelSourceReader, IDisposable
    {
        private bool disposedValue;
        private Stream _stream;
        private readonly bool _leaveOpen;
        private readonly string _filePath;
        /// <inheritdoc/>
        public string FilePath { get; set; }
        /// <inheritdoc/>
        public string SheetName { get; set; }
        /// <summary>
        /// Constructor para leer desde una ruta de archivo física.
        /// </summary>
        /// <param name="path">Ruta completa al archivo .xlsx.</param>
        public ExcelSourceReader(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("No se encontró el archivo Excel.", path);

            _filePath = path;
            // Se abrirá el stream en el momento de la lectura para no bloquear el archivo prematuramente.
        }
        /// <inheritdoc/>
        public bool CanRead => !string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(SheetName);

        /// <inheritdoc/>
        public bool HasHeader { get; set; } = true;
        /// <inheritdoc/>
        public List<string> GetSheets()
        {
            // Aseguramos que el stream esté disponible
            EnsureStreamOpen();

            using var workbook = new XLWorkbook(_stream);
            return [.. workbook.Worksheets.Select(x => x.Name)];
        }

        public DataTable ReadData()
        {
            // Aseguramos que el stream esté disponible
            EnsureStreamOpen();
            DataTable dtResult = new DataTable();

            using var workbook = new XLWorkbook(_stream);

            IXLWorksheet worksheet;

            // Selección de la hoja de trabajo
            if (!string.IsNullOrWhiteSpace(SheetName))
            {
                if (!workbook.Worksheets.TryGetWorksheet(SheetName, out worksheet))
                {
                    throw new DataLoadException($"La hoja de cálculo '{SheetName}' no existe en el archivo.");
                }
            }
            else
            {
                worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) throw new DataLoadException("El archivo Excel no contiene hojas de cálculo.");
            }

            // Obtener solo las filas con datos para evitar iterar millones de filas vacías
            var rows = worksheet.RowsUsed();

            if (!rows.Any())
            {
                return dtResult;
            }

            List<string> headers = new List<string>();
            var rowsEnumerator = rows.GetEnumerator();

            // Manejo de Encabezados
            if (HasHeader)
            {
                if (rowsEnumerator.MoveNext())
                {
                    var headerRow = rowsEnumerator.Current;
                    // Leemos las celdas de la primera fila usada para los encabezados
                    headers = headerRow.Cells().Select(c => c.GetValue<string>()?.Trim()).ToList();
                }
            }
            else
            {
                // Si no hay encabezados, generamos nombres genéricos o dejamos que el ImportManager maneje índices
                // Para consistencia con CsvSourceReader, si se espera header, debe existir.
                // Aquí asumimos que si HasHeader es false, simplemente empezamos a leer datos.
                if (rowsEnumerator.MoveNext())
                {
                    int cellCount = rowsEnumerator.Current.LastCellUsed()?.Address.ColumnNumber ?? 0;
                    rowsEnumerator.Reset();
                    for (int i = 0; i < cellCount; i++)
                        headers.Add($"Col_{i + 1}");
                }
            }
            foreach (string item in headers)
            {
                dtResult.Columns.Add(item, typeof(string));
            }
            // Iterar sobre las filas de datos
            while (rowsEnumerator.MoveNext())
            {
                var row = rowsEnumerator.Current;
                // Si la fila está completamente vacía, la saltamos (ClosedXML RowsUsed suele manejar esto, pero es doble seguridad)
                if (row.IsEmpty()) continue;

                // Iteramos basándonos en el número de celdas de la fila actual o el número de headers
                // Usamos el índice máximo para cubrir casos donde hay datos más allá de los headers o viceversa.
                int cellCount = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
                int maxIndex = Math.Max(cellCount, headers.Count);
                DataRow dtRow = dtResult.NewRow();

                for (int i = 1; i <= maxIndex; i++) // ClosedXML usa índices base-1 para columnas
                {
                    string header = headers[i - 1];
                    // Obtener la celda de manera segura
                    var cell = row.Cell(i);
                    string value = GetCellValueAsString(cell);
                    dtRow[header] = value;
                }
            }

            return dtResult;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_stream != null)
                        _stream.Dispose();
                    _stream = null;
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~ExcelSourceReader()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Convierte el valor de una celda a su representación en cadena de forma segura.
        /// </summary>
        private string GetCellValueAsString(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return string.Empty;

            switch (cell.DataType)
            {
                case XLDataType.Text:
                    return cell.GetValue<string>()?.Trim();

                case XLDataType.Boolean:
                    return cell.GetValue<bool>().ToString();

                case XLDataType.DateTime:
                    // Devolvemos el formato estándar ISO o el que tenga la celda si se prefiere
                    // cell.GetFormattedString() respeta el formato visual de Excel, lo cual suele ser preferible para importaciones.
                    return cell.GetFormattedString();

                case XLDataType.Number:
                    return cell.GetValue<double>().ToString(System.Globalization.CultureInfo.InvariantCulture);

                case XLDataType.TimeSpan:
                    return cell.GetValue<TimeSpan>().ToString();

                default:
                    return cell.GetFormattedString();
            }
        }
        private void EnsureStreamOpen()
        {
            if (_stream != null) return;

            if (!string.IsNullOrEmpty(FilePath))
            {
                _stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            else
            {
                throw new InvalidOperationException("No se ha proporcionado un Stream ni una ruta de archivo válida.");
            }
        }
    }
}
