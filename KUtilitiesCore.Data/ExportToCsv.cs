using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    public static class ExportToCsv
    {
        /// <summary>
        /// Exporta un DataTable a un archivo CSV delimitado por comas.
        /// </summary>
        public static void ToCsv(this DataTable dataSource, string filePath, bool openFile = true, string separator = ",")
        {
            if (dataSource == null) throw new ArgumentNullException(nameof(dataSource));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Ruta inválida", nameof(filePath));

            // Filtrar columnas excluidas
            var columnsToExport = dataSource.Columns.Cast<DataColumn>()
                                    .Where(c => !c.IsExcluded())
                                    .ToList();

            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // 1. Escribir Encabezados
                var headers = columnsToExport.Select(c => EscapeCsvValue(
                    !string.IsNullOrWhiteSpace(c.Caption) ? c.Caption : c.ColumnName, separator)
                );
                sw.WriteLine(string.Join(separator, headers));

                // 2. Escribir Filas
                foreach (DataRow row in dataSource.Rows)
                {
                    var fields = columnsToExport.Select(c =>
                    {
                        var value = row[c];
                        string stringValue = value == DBNull.Value || value == null
                            ? ""
                            : GetFormattedValue(value, c); // Usa el formato si existe
                        return EscapeCsvValue(stringValue, separator);
                    });

                    sw.WriteLine(string.Join(separator, fields));
                }
            }

            if (openFile)
            {
                ExportUtils.OpenFile(filePath);
            }
        }

        private static string GetFormattedValue(object value, DataColumn column)
        {
            // Intentar usar el formato definido en las extensiones
            string format = column.GetDisplayFormat();

            if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
            {
                return formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
            }

            // Fallback especial para fechas si no hay formato
            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss");

            return value.ToString();
        }

        /// <summary>
        /// Escapa los valores para cumplir con el estándar CSV (RFC 4180).
        /// Maneja comillas, separadores y saltos de línea.
        /// </summary>
        private static string EscapeCsvValue(string value, string separator)
        {
            if (string.IsNullOrEmpty(value)) return "";

            bool needsQuotes = value.Contains(separator) ||
                               value.Contains("\"") ||
                               value.Contains("\r") ||
                               value.Contains("\n");

            if (needsQuotes)
            {
                // Duplicar comillas dobles existentes
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
    }
}
