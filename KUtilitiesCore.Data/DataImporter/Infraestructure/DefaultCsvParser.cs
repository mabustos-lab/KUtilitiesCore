using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace KUtilitiesCore.Data.DataImporter.Infrastructure
{
    /// <summary>
    /// Implementación por defecto del parser CSV
    /// </summary>
    public class DefaultCsvParser : ICsvParser
    {
        /// <inheritdoc/>
        public DataTable Parse(Stream stream, TextFileParsingOptions options)
        {
            using var reader = new StreamReader(stream, options.Encoding);
            return ParseInternal(reader, options);
        }

        /// <inheritdoc/>
        public async Task<DataTable> ParseAsync(Stream stream, TextFileParsingOptions options)
        {
            using var reader = new StreamReader(stream, options.Encoding);
            return await Task.Run(() => ParseInternal(reader, options));
        }

        /// <summary>
        /// Lógica principal de parsing
        /// </summary>
        private DataTable ParseInternal(StreamReader reader, TextFileParsingOptions options)
        {
            var dataTable = new DataTable();
            var columnNames = new List<string>();
            var columnMapping = new Dictionary<int, int>();
            bool headerProcessed = !options.HasHeader;

            // Si no hay encabezado, crear columnas automáticamente
            if (!options.HasHeader)
            {
                CreateAutoColumns(dataTable, reader, options);
                headerProcessed = true;
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (options.IgnoreEmptyLines && string.IsNullOrWhiteSpace(line))
                    continue;

                string[] values = SplitLine(line, options);

                if (!headerProcessed)
                {
                    ProcessHeader(values, dataTable, columnNames, columnMapping, options);
                    headerProcessed = true;
                }
                else
                {
                    ProcessDataRow(values, dataTable, columnMapping, options);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Divide una línea según las opciones de parsing
        /// </summary>
        private string[] SplitLine(string line, TextFileParsingOptions options)
        {
            if (options.EscapeCharacter.HasValue)
            {
                return SplitWithEscape(line, options.Separator, options.EscapeCharacter.Value);
            }

            return line.Split(new[] { options.Separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Divide una línea considerando caracteres de escape
        /// </summary>
        private string[] SplitWithEscape(string line, string separator, char escapeChar)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inEscape = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == escapeChar)
                {
                    inEscape = !inEscape;
                    continue;
                }

                if (!inEscape && i + separator.Length <= line.Length &&
                    line.Substring(i, separator.Length) == separator)
                {
                    result.Add(current.ToString());
                    current.Clear();
                    i += separator.Length - 1;
                    continue;
                }

                current.Append(c);
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        /// <summary>
        /// Procesa la línea de encabezado
        /// </summary>
        private void ProcessHeader(string[] values, DataTable dataTable,
            List<string> columnNames, Dictionary<int, int> columnMapping, TextFileParsingOptions options)
        {
            for (int i = 0; i < values.Length; i++)
            {
                string columnName = options.TrimValues ? values[i].Trim() : values[i];

                // Asignar nombre por defecto si está vacío
                if (string.IsNullOrEmpty(columnName))
                    columnName = $"Column{i + 1}";

                // Manejar nombres duplicados manteniendo el primero
                if (!columnNames.Any(name => options.ColumnNameComparer.Equals(name, columnName)))
                {
                    columnNames.Add(columnName);
                    dataTable.Columns.Add(columnName, typeof(string));
                    columnMapping[i] = columnNames.Count - 1;
                }
                else
                {
                    // Para duplicados, usar la primera columna encontrada
                    int existingIndex = columnNames.FindIndex(name =>
                        options.ColumnNameComparer.Equals(name, columnName));
                    columnMapping[i] = existingIndex;
                }
            }
        }

        /// <summary>
        /// Procesa una fila de datos
        /// </summary>
        private void ProcessDataRow(string[] values, DataTable dataTable,
            Dictionary<int, int> columnMapping, TextFileParsingOptions options)
        {
            DataRow row = dataTable.NewRow();

            for (int i = 0; i < values.Length; i++)
            {
                if (columnMapping.TryGetValue(i, out int targetColumnIndex))
                {
                    string value = options.TrimValues ? values[i].Trim() : values[i];
                    row[targetColumnIndex] = value == string.Empty ? DBNull.Value : value;
                }
            }

            // Solo agregar fila si no está completamente vacía
            if (!IsEmptyRow(row))
            {
                dataTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// Crea columnas automáticamente cuando no hay encabezado
        /// </summary>
        private void CreateAutoColumns(DataTable dataTable, StreamReader reader, TextFileParsingOptions options)
        {
            var firstLine = reader.ReadLine();
            if (!string.IsNullOrEmpty(firstLine))
            {
                var values = SplitLine(firstLine, options);

                // Crear columnas con nombres por defecto
                for (int i = 0; i < values.Length; i++)
                {
                    dataTable.Columns.Add($"Column{i + 1}", typeof(string));
                }

                // Procesar la primera línea como datos
                var columnMapping = CreateSequentialMapping(values.Length);
                ProcessDataRow(values, dataTable, columnMapping, options);
            }
        }

        /// <summary>
        /// Crea un mapeo secuencial de índices
        /// </summary>
        private Dictionary<int, int> CreateSequentialMapping(int length)
        {
            var mapping = new Dictionary<int, int>();
            for (int i = 0; i < length; i++)
            {
                mapping[i] = i;
            }
            return mapping;
        }

        /// <summary>
        /// Determina si una fila está completamente vacía
        /// </summary>
        private bool IsEmptyRow(DataRow row)
        {
            foreach (var item in row.ItemArray)
            {
                if (item != DBNull.Value && !string.IsNullOrWhiteSpace(item?.ToString()))
                    return false;
            }
            return true;
        }
    }
}
