using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace KUtilitiesCore.Extensions
{
    public static class DataTableExt
    {
        #region Métodos de extensión

        /// <summary>
        /// Obtiene las columnas de un DataTable como enumeración
        /// </summary>
        public static IEnumerable<DataColumn> GetColumns(this DataTable dataTable)
        {
            return dataTable.Columns.Cast<DataColumn>();
        }

        /// <summary>
        /// Convierte un DataTable en texto estructurado con separadores personalizables
        /// </summary>
        public static string ToText(this DataTable dataTable, bool includeHeader = true, string separator = "\t")
        {
            var output = new StringBuilder();
            var columns = dataTable.GetColumns().ToList();

            if (includeHeader)
            {
                output.AppendLine(FormatHeader(columns, separator));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                output.AppendLine(FormatRow(row, columns, separator));
            }

            return output.ToString();
        }

        /// <summary>
        /// Serializa un DataTable a formato XML
        /// </summary>
        public static XDocument ToXml(this DataTable dataTable, string rootName = "Data")
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            if (string.IsNullOrWhiteSpace(rootName)) rootName = "Data";

            var rootElement = new XElement(rootName);
            var xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);

            foreach (DataRow row in dataTable.Rows)
            {
                var rowElement = new XElement(dataTable.TableName.DefaultIfEmpty("Row"));
                foreach (DataColumn column in dataTable.Columns)
                {
                    var cellValue = (row[column].ToString() ?? string.Empty).Trim();
                    rowElement.Add(new XElement(column.ColumnName.SanitizeXmlName(), cellValue));
                }
                rootElement.Add(rowElement);
            }

            return xmlDocument;
        }

        /// <summary>
        /// Mapea un DataTable a una colección de objetos
        /// </summary>
        public static IEnumerable<T> MapTo<T>(this DataTable dataTable) where T : class, new()
        {
            var propertyCache = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanWrite)
                                        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dataTable.Rows)
            {
                yield return CreateEntity<T>(row, propertyCache);
            }
        }

        /// <summary>
        /// Convierte un objeto en DataTable
        /// </summary>
        public static DataTable ToDataTable<TSource>(this TSource source) where TSource : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var dataTable = new DataTable();
            var type = typeof(TSource);
            var properties = type.GetRuntimeProperties().ToList();

            foreach (var property in properties)
            {
                var columnType = GetUnderlyingType(property.PropertyType);
                dataTable.Columns.Add(property.Name, columnType);
            }

            var dataRow = dataTable.NewRow();
            foreach (var property in properties)
            {
                dataRow[property.Name] = property.GetValue(source) ?? DBNull.Value;
            }

            dataTable.Rows.Add(dataRow);
            return dataTable;
        }

        #endregion

        #region Helpers

        private static string FormatHeader(IEnumerable<DataColumn> columns, string separator)
        {
            return string.Join(separator, columns.Select(c => c.ColumnName));
        }

        private static string FormatRow(DataRow row, IEnumerable<DataColumn> columns, string separator)
        {
            var formattedValues = columns.Select(column =>
            {
                var value = row[column].ToString()??string.Empty;
                return value.Contains(separator) ? $"\"{value}\"" : value;
            });

            return string.Join(separator, formattedValues);
        }

        private static T CreateEntity<T>(DataRow row, IReadOnlyDictionary<string, PropertyInfo> properties) where T : new()
        {
            var entity = new T();
            foreach (var property in properties.Values)
            {
                if (!row.Table.Columns.Contains(property.Name)) continue;

                var value = row[property.Name];
                if (value == DBNull.Value) continue;

                var targetType = GetUnderlyingType(property.PropertyType);
                var convertedValue = ConvertValue(value, targetType);
                property.SetValue(entity, convertedValue);
            }
            return entity;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString()??string.Empty);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static string SanitizeXmlName(this string name)
        {
            return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        private static string DefaultIfEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        #endregion
    }
}