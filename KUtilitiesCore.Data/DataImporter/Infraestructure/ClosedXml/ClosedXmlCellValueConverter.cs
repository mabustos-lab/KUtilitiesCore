using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Implementación de ICellValueConverter para ClosedXML
    /// </summary>
    public class ClosedXmlCellValueConverter : ICellValueConverter
    {
        private readonly ExcelParsingOptions _options;

        public ClosedXmlCellValueConverter(ExcelParsingOptions options)
        {
            _options = options ?? new ExcelParsingOptions();
        }

        /// <inheritdoc/>
        public string ConvertToString(IExcelCell cell)
        {
            if (cell == null || cell.IsEmpty)
                return _options.TreatEmptyAsNull ? null : string.Empty;

            string value = cell.FormattedValue;

            if (_options.TrimValues && value != null)
            {
                value = value.Trim();
            }

            // Si después de trim está vacío y se debe tratar como null
            if (string.IsNullOrEmpty(value) && _options.TreatEmptyAsNull)
            {
                return null;
            }

            return value;
        }
    }
}
