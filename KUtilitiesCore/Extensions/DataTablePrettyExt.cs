using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Extensiones para imprimir el contenido de un DataSet
    /// </summary>
    public static class DataTablePrettyExt
    {
        /// <summary>
        /// Imprime el contenido de un DataSet en formato tabular legible.
        /// </summary>
        /// <param name="dataSet">El DataSet a imprimir.</param>
        /// <param name="useDebug">Si es true, usa Debug.WriteLine; si es false (por defecto), usa Console.WriteLine.</param>
        public static void PrintPretty(this DataSet dataSet, bool useDebug = false)
        {
            // Definimos la acción de salida (Consola o Debug)
            Action<string> output = useDebug ? (Action<string>)(msg => Debug.WriteLine(msg)) : Console.WriteLine;

            if (dataSet == null)
            {
                output("El DataSet es NULL.");
                return;
            }

            output($"=== DataSet: {dataSet.DataSetName} ===");

            foreach (DataTable table in dataSet.Tables)
            {
                PrintDataTable(table, output);
                output(""); // Espacio entre tablas
            }
        }

        /// <summary>
        /// Método auxiliar para imprimir un DataTable en consola o Debug.
        /// </summary>
        public static void PrintPretty(this DataTable table, bool useDebug = false)
        {
            Action<string> output = useDebug ? (Action<string>)(msg => Debug.WriteLine(msg)) : Console.WriteLine;
            PrintDataTable(table, output);
        }

        private static void PrintDataTable(DataTable table, Action<string> output)
        {
            if (table == null) return;

            output($"--- Tabla: {table.TableName} ({table.Rows.Count} filas) ---");

            // 1. Obtener las columnas
            var columns = table.Columns.Cast<DataColumn>().ToArray();

            if (columns.Length == 0)
            {
                output("  [Sin Columnas]");
                return;
            }

            // 2. Calcular el ancho máximo de cada columna
            // Revisamos tanto el nombre de la columna como el contenido de todas las filas
            var columnWidths = new Dictionary<string, int>();

            foreach (var col in columns)
            {
                // Empezamos con el largo del nombre de la columna
                int maxLength = col.ColumnName.Length;

                // Revisamos los datos para ver si hay algo más largo
                foreach (DataRow row in table.Rows)
                {
                    string cellValue = row[col] != DBNull.Value ? row[col].ToString() : "NULL";
                    if (cellValue.Length > maxLength)
                    {
                        maxLength = cellValue.Length;
                    }
                }

                // Agregamos un pequeño padding (margen) extra
                columnWidths[col.ColumnName] = maxLength + 2;
            }

            // 3. Construir e imprimir el Encabezado
            StringBuilder headerLine = new StringBuilder();
            StringBuilder separatorLine = new StringBuilder();

            foreach (var col in columns)
            {
                string fmtCol = col.ColumnName.PadRight(columnWidths[col.ColumnName]);
                headerLine.Append(fmtCol).Append("| ");
                separatorLine.Append(new string('-', columnWidths[col.ColumnName])).Append("|-");
            }

            output(headerLine.ToString());
            output(separatorLine.ToString());

            // 4. Construir e imprimir las Filas
            foreach (DataRow row in table.Rows)
            {
                StringBuilder rowLine = new StringBuilder();
                foreach (var col in columns)
                {
                    string cellValue = row[col] != DBNull.Value ? row[col].ToString() : "NULL";

                    // Alineación: Números a la derecha, texto a la izquierda (Opcional, aquí todo a la derecha para simplicidad)
                    // Usamos PadRight para mantener la estructura de columnas
                    rowLine.Append(cellValue.PadRight(columnWidths[col.ColumnName])).Append("| ");
                }
                output(rowLine.ToString());
            }
        }
    }
}
