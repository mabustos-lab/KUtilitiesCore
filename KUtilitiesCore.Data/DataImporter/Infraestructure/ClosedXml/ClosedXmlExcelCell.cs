using ClosedXML.Excel;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Implementación de IExcelCell usando ClosedXML
    /// </summary>
    public class ClosedXmlExcelCell : IExcelCell
    {
        private readonly IXLCell _cell;

        /// <inheritdoc/>
        public string Address => _cell.Address.ToString();
        /// <inheritdoc/>
        public int ColumnNumber => _cell.Address.ColumnNumber;
        /// <inheritdoc/>
        public object RawValue => _cell.Value;
        /// <inheritdoc/>
        public bool IsEmpty => _cell.IsEmpty();
        /// <inheritdoc/>
        public string DataType => _cell.DataType.ToString();
        /// <inheritdoc/>
        public string FormattedValue => GetFormattedValue();

        public ClosedXmlExcelCell(IXLCell cell)
        {
            _cell = cell;
        }

        private string GetFormattedValue()
        {
            if (_cell.IsEmpty())
                return string.Empty;

            switch (_cell.DataType)
            {
                case XLDataType.Text:
                    return _cell.GetText();

                case XLDataType.Boolean:
                    return _cell.GetBoolean().ToString();

                case XLDataType.DateTime:
                    return _cell.GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");

                case XLDataType.Number:
                    return _cell.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture);

                case XLDataType.TimeSpan:
                    return _cell.GetTimeSpan().ToString();

                default:
                    return _cell.GetFormattedString();
            }
        }
    }
}
