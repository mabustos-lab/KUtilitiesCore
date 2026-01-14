using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace KUtilitiesCore.Data.DataExporter
{
    /// <summary>
    /// Exporta un DataTable a un archivo Excel (.xlsx).
    /// </summary>
    public static class ExportToExcel
    {
        /// <summary>
        /// Exporta un DataTable a un archivo Excel (.xlsx).
        /// </summary>
        public static void ToExcel(this DataTable dataSource, string filePath, bool openFile = true)
        {
            ValidateParameters(dataSource, filePath);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = CreateWorksheet(workbook, dataSource);
                var columnsToExport = GetExportableColumns(dataSource);

                WriteHeaders(worksheet, columnsToExport);
                WriteDataRows(worksheet, dataSource, columnsToExport);
                ApplyColumnSettings(worksheet, columnsToExport);

                workbook.SaveAs(filePath);
            }

            if (openFile)
            {
                ExportUtils.OpenFile(filePath);
            }
        }

        private static void ValidateParameters(DataTable dataSource, string filePath)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            if (dataSource.Rows.Count == 0)
                throw new EmptyDataSourceException("El origen de datos está vacío.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("La ruta del archivo no es válida.", nameof(filePath));
        }

        private static IXLWorksheet CreateWorksheet(XLWorkbook workbook, DataTable dataSource)
        {
            string sheetName = ExportUtils.GetValidSheetName(dataSource.TableName);
            return workbook.Worksheets.Add(sheetName);
        }

        private static List<DataColumn> GetExportableColumns(DataTable dataSource)
        {
            return dataSource.Columns.Cast<DataColumn>()
                            .Where(c => !c.IsExcluded())
                            .ToList();
        }

        private static void WriteHeaders(IXLWorksheet worksheet, List<DataColumn> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var cell = worksheet.Cell(1, i + 1);
                string headerText = GetColumnHeaderText(column);

                FormatHeaderCell(cell, headerText);
                AddHeaderTooltip(cell, column, headerText);
            }
        }

        private static string GetColumnHeaderText(DataColumn column)
        {
            return !string.IsNullOrWhiteSpace(column.Caption)
                ? column.Caption
                : column.ColumnName;
        }

        private static void FormatHeaderCell(IXLCell cell, string headerText)
        {
            cell.Value = headerText;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        private static void AddHeaderTooltip(IXLCell cell, DataColumn column, string headerText)
        {
            string description = column.GetDescription();
            if (!string.IsNullOrWhiteSpace(description))
            {
                cell.GetDataValidation().InputTitle = $"Info: {headerText}";
                cell.GetDataValidation().InputMessage = description;
            }
        }

        private static void WriteDataRows(IXLWorksheet worksheet, DataTable dataSource, List<DataColumn> columns)
        {
            for (int rowIndex = 0; rowIndex < dataSource.Rows.Count; rowIndex++)
            {
                var row = dataSource.Rows[rowIndex];
                WriteDataRow(worksheet, row, columns, rowIndex + 2);
            }

            worksheet.Columns().AdjustToContents();
        }

        private static void WriteDataRow(IXLWorksheet worksheet, DataRow row, List<DataColumn> columns, int excelRowIndex)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var cell = worksheet.Cell(excelRowIndex, i + 1);

                SetCellValue(cell, row[column]);
                ApplyCellFormatting(cell, column, row[column]);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private static void SetCellValue(IXLCell cell, object value)
        {
            if (value == DBNull.Value || value == null)
            {
                cell.Value = string.Empty;
            }
            else
            {
                cell.Value = XLCellValue.FromObject(value);
            }
        }

        private static void ApplyCellFormatting(IXLCell cell, DataColumn column, object value)
        {
            string format = column.GetXLDisplayFormat();
            if (!string.IsNullOrEmpty(format))
            {
                cell.Style.NumberFormat.Format = format;
            }
            else if (value is DateTime)
            {
                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
            }
        }

        private static void ApplyColumnSettings(IXLWorksheet worksheet, List<DataColumn> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var excelColumn = worksheet.Column(i + 1);

                ApplyColumnWidth(excelColumn, column);
            }
        }

        private static void ApplyColumnWidth(IXLColumn excelColumn, DataColumn column)
        {
            double customWidth = column.GetXLColumnWidth();
            if (customWidth > 0)
            {
                excelColumn.Width = customWidth;
            }
        }
    }
}