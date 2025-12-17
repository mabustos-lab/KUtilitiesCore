using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
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
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (dataSource.Rows.Count == 0) throw new EmptyDataSourceException("El origen de datos está vacío.");
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("La ruta del archivo no es válida.", nameof(filePath));

            using (var workbook = new XLWorkbook())
            {
                string sheetName = ExportUtils.GetValidSheetName(dataSource.TableName);
                var worksheet = workbook.Worksheets.Add(sheetName);

                // Filtrar columnas excluidas
                var columnsToExport = dataSource.Columns.Cast<DataColumn>()
                                        .Where(c => !c.IsExcluded())
                                        .ToList();

                // 1. Encabezados
                int colIndex = 1;
                foreach (var column in columnsToExport)
                {
                    var cell = worksheet.Cell(1, colIndex);
                    string headerText = !string.IsNullOrWhiteSpace(column.Caption) ? column.Caption : column.ColumnName;

                    // Aplicar formato al encabezado
                    cell.Value = headerText;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    
                    // Tooltip
                    string description = column.GetDescription();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        cell.GetDataValidation().InputTitle = $"Info: {headerText}";
                        cell.GetDataValidation().InputMessage = description;
                        //// Permitir cualquier valor (solo queremos el mensaje)
                        //validation.IgnoreBlanks = true;
                    }
                    colIndex++;
                }
                // 2. Datos
                int rowIndex = 2;
                foreach (DataRow row in dataSource.Rows)
                {
                    colIndex = 1;
                    foreach (var column in columnsToExport)
                    {
                        var cell = worksheet.Cell(rowIndex, colIndex);
                        var value = row[column];

                        if (value == DBNull.Value || value == null)
                        {
                            cell.Value = string.Empty;
                        }
                        else
                        {
                            // Asignación inteligente de ClosedXML (detecta tipos automáticamente mejor que ifs manuales)
                            cell.Value = XLCellValue.FromObject(value);

                            // Aplicar formato personalizado si existe
                            string format = column.GetDisplayFormat();
                            if (!string.IsNullOrEmpty(format))
                            {
                                cell.Style.NumberFormat.Format = format;
                            }
                            // Formato por defecto para fechas si no se especificó uno
                            else if (value is DateTime)
                            {
                                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                            }
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            colIndex++;
                        }
                        rowIndex++;
                    }
                    worksheet.Columns().AdjustToContents();

                    ApplyColumnWidth(worksheet, columnsToExport);
                    
                    workbook.SaveAs(filePath);
                }
            }

            if (openFile)
            {
                ExportUtils.OpenFile(filePath);
            }
        }
        private static void ApplyColumnWidth(IXLWorksheet worksheet, List<DataColumn> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var excelColumn = worksheet.Column(i + 1);

                // Aplicar ancho personalizado si está configurado
                double customWidth = column.GetColumnWidth();
                if (customWidth > 0)
                {
                    excelColumn.Width = customWidth;
                }
            }
        }
    }
}
