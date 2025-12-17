using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace KUtilitiesCore.Data
{
    public static class ExportToCsv
    {
        /// <summary>
        /// Exporta un DataTable a un archivo CSV delimitado por comas.
        /// </summary>
        public static void ToCsv(this DataTable dataSource, string filePath, bool openFile = true, string separator = ",")
        {
            ValidateParameters(dataSource, filePath, separator);
            var columnsToExport = GetExportableColumns(dataSource);

            WriteCsvFile(filePath, dataSource, columnsToExport, separator);

            if (openFile)
            {
                ExportUtils.OpenFile(filePath);
            }
        }

        private static void ValidateParameters(DataTable dataSource, string filePath, string separator)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Ruta inválida", nameof(filePath));

            if (string.IsNullOrWhiteSpace(separator))
                throw new ArgumentException("Separador no puede estar vacío", nameof(separator));
        }

        private static List<DataColumn> GetExportableColumns(DataTable dataSource)
        {
            return dataSource.Columns.Cast<DataColumn>()
                                    .Where(c => !c.IsExcluded())
                                    .ToList();
        }

        private static void WriteCsvFile(string filePath, DataTable dataSource, List<DataColumn> columns, string separator)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                WriteHeaders(writer, columns, separator);
                WriteDataRows(writer, dataSource, columns, separator);
            }
        }

        private static void WriteHeaders(StreamWriter writer, List<DataColumn> columns, string separator)
        {
            var headerValues = columns.Select(c => GetColumnHeader(c));
            var escapedHeaders = headerValues.Select(h => EscapeCsvValue(h, separator));

            writer.WriteLine(string.Join(separator, escapedHeaders));
        }

        private static string GetColumnHeader(DataColumn column)
        {
            return !string.IsNullOrWhiteSpace(column.Caption)
                ? column.Caption
                : column.ColumnName;
        }

        private static void WriteDataRows(StreamWriter writer, DataTable dataSource, List<DataColumn> columns, string separator)
        {
            foreach (DataRow row in dataSource.Rows)
            {
                WriteDataRow(writer, row, columns, separator);
            }
        }

        private static void WriteDataRow(StreamWriter writer, DataRow row, List<DataColumn> columns, string separator)
        {
            var fieldValues = columns.Select(c => GetFormattedFieldValue(row[c], c));
            var escapedFields = fieldValues.Select(v => EscapeCsvValue(v, separator));

            writer.WriteLine(string.Join(separator, escapedFields));
        }

        private static string GetFormattedFieldValue(object value, DataColumn column)
        {
            if (value == DBNull.Value || value == null)
                return string.Empty;

            return GetFormattedValue(value, column);
        }

        private static string GetFormattedValue(object value, DataColumn column)
        {
            string format = column.GetDisplayFormat();

            if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
            {
                return formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
            }

            if (value is DateTime dateTime)
                return FormatDateTime(dateTime);

            return value.ToString();
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Escapa los valores para cumplir con el estándar CSV (RFC 4180).
        /// Maneja comillas, separadores y saltos de línea.
        /// </summary>
        private static string EscapeCsvValue(string value, string separator)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (RequiresQuoting(value, separator))
                return QuoteAndEscapeValue(value);

            return value;
        }

        private static bool RequiresQuoting(string value, string separator)
        {
            return value.Contains(separator) ||
                   value.Contains("\"") ||
                   value.Contains("\r") ||
                   value.Contains("\n");
        }

        private static string QuoteAndEscapeValue(string value)
        {
            // Duplicar comillas dobles existentes
            string escapedValue = value.Replace("\"", "\"\"");
            return $"\"{escapedValue}\"";
        }
    }
}