using ClosedXML.Excel;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Infraestructure.ClosedXml
{
    /// <summary>
    /// Implementación de IExcelRow usando ClosedXML
    /// </summary>
    public class ClosedXmlExcelRow : IExcelRow
    {
        private readonly IXLRow _row;

        /// <inheritdoc/>
        public int RowNumber => _row.RowNumber();
        /// <inheritdoc/>
        public bool IsEmpty => _row.IsEmpty();

        public ClosedXmlExcelRow(IXLRow row)
        {
            _row = row;
        }

        /// <inheritdoc/>
        public IEnumerable<IExcelCell> Cells
        {
            get
            {
                var usedCells = _row.CellsUsed();
                foreach (var cell in usedCells)
                {
                    yield return new ClosedXmlExcelCell(cell);
                }
            }
        }
    }
}
