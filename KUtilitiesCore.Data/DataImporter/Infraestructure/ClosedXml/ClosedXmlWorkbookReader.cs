using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Implementación de IExcelWorkbookReader usando ClosedXML
    /// </summary>
    public class ClosedXmlWorkbookReader : IExcelWorkbookReader
    {
        private readonly XLWorkbook _workbook;
        private readonly Stream _stream;
        private readonly bool _ownsStream;
        private bool disposedValue;

        /// <summary>
        /// Crea un lector desde un stream
        /// </summary>
        public ClosedXmlWorkbookReader(Stream stream, bool ownsStream = true)
        {
            _stream = stream;
            _ownsStream = ownsStream;
            _workbook = new XLWorkbook(stream);
        }

        /// <summary>
        /// Crea un lector desde un archivo
        /// </summary>
        public ClosedXmlWorkbookReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Archivo Excel no encontrado", filePath);

            _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _ownsStream = true;
            _workbook = new XLWorkbook(_stream);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetSheetNames()
        {
            var sheets = new List<string>();
            foreach (var worksheet in _workbook.Worksheets)
            {
                sheets.Add(worksheet.Name);
            }
            return sheets.AsReadOnly();
        }

        /// <inheritdoc/>
        public IExcelWorksheetReader GetWorksheet(string sheetName)
        {
            var worksheet = _workbook.Worksheets.Worksheet(sheetName);
            if (worksheet == null)
                throw new ArgumentException($"La hoja '{sheetName}' no existe en el archivo");

            return new ClosedXmlWorksheetReader(worksheet);
        }

        /// <inheritdoc/>
        public IExcelWorksheetReader GetFirstWorksheet()
        {
            var worksheet = _workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new InvalidOperationException("El archivo Excel no contiene hojas");

            return new ClosedXmlWorksheetReader(worksheet);
        }

        /// <inheritdoc/>
        public bool ContainsSheet(string sheetName)
        {
            return _workbook.Worksheets.TryGetWorksheet(sheetName, out _);
        }

        /// <summary>
        /// Libera el libro de ClosedXML y, si es propietario del stream,
        /// también lo cierra. Se invoca desde <see cref="Dispose()"/> y,
        /// opcionalmente, desde el finalizador.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> cuando la liberación proviene de una llamada explícita a
        /// <see cref="Dispose()"/>; <c>false</c> cuando proviene del finalizador.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _workbook?.Dispose();
                    if (_ownsStream && _stream != null)
                    {
                        _stream.Dispose();
                    }
                }
                
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~ClosedXmlWorkbookReader()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Libera todos los recursos del lector invocando a
        /// <see cref="Dispose(bool)"/> con <c>disposing</c> en <c>true</c>.
        /// </summary>
        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
