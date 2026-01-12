using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Factory para crear lectores ClosedXML
    /// </summary>
    public class ClosedXmlWorkbookReaderFactory : IExcelWorkbookReaderFactory
    {
        private static readonly string[] SupportedExtensions = { ".xlsx", ".xlsm" };

        /// <inheritdoc/>
        public IExcelWorkbookReader CreateFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Archivo Excel no encontrado", filePath);

            return new ClosedXmlWorkbookReader(filePath);
        }
        /// <inheritdoc/>
        public IExcelWorkbookReader CreateFromStream(Stream stream, bool leaveOpen = false)
        {
            return new ClosedXmlWorkbookReader(stream, !leaveOpen);
        }
        /// <inheritdoc/>
        public bool SupportsFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return SupportedExtensions.Contains(extension);
        }
    }
}
