using ClosedXML.Excel;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Implementación de IExcelWorksheetReader usando ClosedXML
    /// </summary>
    public class ClosedXmlWorksheetReader : IExcelWorksheetReader
    {
        private readonly IXLWorksheet _worksheet;

        /// <inheritdoc/>
        public string Name => _worksheet.Name;
        /// <inheritdoc/>
        public int RowCount => _worksheet.RowsUsed().Count();
        /// <inheritdoc/>
        public int ColumnCount => _worksheet.ColumnsUsed().Count();

        public ClosedXmlWorksheetReader(IXLWorksheet worksheet)
        {
            _worksheet = worksheet ?? throw new ArgumentNullException(nameof(worksheet));
        }

        /// <inheritdoc/>
        public IEnumerable<IExcelRow> ReadRows()
        {
            var usedRows = _worksheet.RowsUsed();
            foreach (var row in usedRows)
            {
                yield return new ClosedXmlExcelRow(row);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IExcelRow> ReadRows(int startRow, int endRow)
        {
            for (int rowNum = startRow; rowNum <= endRow; rowNum++)
            {
                var row = _worksheet.Row(rowNum);
                if (row.IsEmpty())
                    continue;

                yield return new ClosedXmlExcelRow(row);
            }
        }
    }
}
