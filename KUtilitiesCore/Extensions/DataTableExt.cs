using KUtilitiesCore.Data.Converter;
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

        /// <summary>
        /// Obtiene las columnas de un DataTable como enumeración
        /// </summary>
        public static IEnumerable<DataColumn> GetColumns(this DataTable dataTable)
        {
            return dataTable.Columns.Cast<DataColumn>();
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
        /// Convierte una colección de objetos en un DataTable
        /// </summary>
        /// <typeparam name="TSource">Tipo de los elementos en la colección</typeparam>
        /// <param name="source">Colección de elementos a convertir</param>
        /// <param name="onRowAdded">Callback opcional a ejecutar después de agregar cada fila</param>
        /// <returns>DataTable con la estructura y datos de la colección</returns>
        /// <exception cref="ArgumentNullException">
        /// Se lanza cuando el parámetro source es nulo
        /// </exception>
        public static DataTable ToDataTable<TSource>(this IEnumerable<TSource> source,
            Action<DataRow, TSource>? onRowAdded = null) where TSource : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var dataTable = CreateDataTableStructure<TSource>();
            PopulateDataTableRows(dataTable, source, onRowAdded);

            return dataTable;
        }

        /// <summary>
        /// Convierte un DataTable en texto estructurado con separadores personalizables
        /// </summary>
        public static string ToText(this DataTable dataTable,
            bool includeHeader = true, string separator = "\t")
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

        private static object ConvertValue(object value, Type targetType, ITypeConverterProvider provider)
        {
            
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString() ?? string.Empty);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static DataRow CreateDataRow<TSource>(DataTable dataTable, TSource item)
        {
            var row = dataTable.NewRow();
            var properties = typeof(TSource).GetRuntimeProperties();

            foreach (var property in properties)
            {
                row[property.Name] = property.GetValue(item) ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
            return row;
        }

        public static DataTable CreateDataTableStructure<TSource>()
        {
            var dataTable = new DataTable();
            var sourceType = typeof(TSource);
            var properties = sourceType.GetRuntimeProperties().ToList();

            foreach (var property in properties)
            {
                var columnType = GetUnderlyingType(property.PropertyType);
                dataTable.Columns.Add(property.Name, columnType);
            }

            return dataTable;
        }

        private static T CreateEntity<T>(DataRow row, IReadOnlyDictionary<string, PropertyInfo> properties) where T : new()
        {
            ITypeConverterProvider provider = Data.Converter.TypeConverterFactory.Provider;
            var entity = new T();
            foreach (var property in properties.Values)
            {
                if (!row.Table.Columns.Contains(property.Name)) continue;

                var value = row[property.Name];
                if (value == DBNull.Value) continue;

                var targetType = GetUnderlyingType(property.PropertyType);
                ITypeConverter typeConverter= provider.Resolve(targetType);
                var convertedValue = typeConverter.TryConvert(value.ToString()??string.Empty);
                property.SetValue(entity, convertedValue);
            }
            return entity;
        }

        private static string DefaultIfEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static string FormatHeader(IEnumerable<DataColumn> columns, string separator)
        {
            return string.Join(separator, columns.Select(c => c.ColumnName));
        }

        private static string FormatRow(DataRow row, IEnumerable<DataColumn> columns, string separator)
        {
            var formattedValues = columns.Select(column =>
            {
                var value = row[column].ToString() ?? string.Empty;
                return value.Contains(separator) ? $"\"{value}\"" : value;
            });

            return string.Join(separator, formattedValues);
        }

        private static Type GetUnderlyingType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static void PopulateDataTableRows<TSource>(DataTable dataTable,
            IEnumerable<TSource> source, Action<DataRow, TSource>? onRowAdded) where TSource : class
        {
            foreach (var item in source)
            {
                var row = CreateDataRow(dataTable, item);
                onRowAdded?.Invoke(row, item);
            }
        }

        private static string SanitizeXmlName(this string name)
        {
            return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

    }
}