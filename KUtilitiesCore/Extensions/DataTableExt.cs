using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KUtilitiesCore.Extensions
{
    public static class DataTableExt
    {
        #region Methods

        /// <summary>
        /// Crea un <see cref="IEnumerable{DataColumn}"/> de las columnas de un <see cref="DataTable"/>/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns><see cref="IEnumerable{DataColumn}" que representa las Columnas</returns>
        public static IEnumerable<DataColumn> DataColumnAsEnumerable(this DataTable dt)
        {
            return dt.Columns.Cast<DataColumn>();
        }

        /// <summary>
        /// Convierte un DataTable en una estructura de texto
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="inclideColumns">Indica que debe incluir las columnas como la primera fla</param>
        /// <param name="separator">establece el separador entre elementos</param>
        /// <returns></returns>
        public static string DataTableToText(this DataTable dt, bool inclideColumns = true, string separator = "\t")
        {
            StringBuilder ret = new StringBuilder();
            IEnumerable<DataColumn> cols = dt.DataColumnAsEnumerable();
            if (inclideColumns) ret.AppendLine(string.Join(separator,
                cols.
                Select(c => c.ColumnName).
                ToArray()));
            foreach (DataRow row in dt.Rows)
            {
                List<string> values = new List<string>();
                foreach (DataColumn col in cols)
                {
                    if (col.DataType == typeof(string))
                    {

                        string res = row[col.ColumnName].ToString();
                        if (res.Contains(separator))
                            res = $"\"{res}\"";
                        values.Add(res);
                    }
                    else
                    {
                        values.Add(row[col.ColumnName].ToString());
                    }
                }
                ret.AppendLine(string.Join(separator, values.ToArray()));
            }
            return ret.ToString();
        }

        /// <summary>
        /// Converts entire DataTabel To XDocument
        ///  example: <example>dtCert.ToXml("Certs")</example>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public static XDocument ToXml(this DataTable dt, string rootName="Root")
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));
            if (string.IsNullOrWhiteSpace(rootName)) throw new ArgumentException("El nombre de la raíz no puede estar vacío.", nameof(rootName));

            var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var rootElement = new XElement(rootName);
            xdoc.Add(rootElement);

            foreach (DataRow row in dt.Rows)
            {
                var rowElement = new XElement(string.IsNullOrEmpty(dt.TableName) ? "Row" : dt.TableName);
                foreach (DataColumn col in dt.Columns)
                {
                    var value = row[col]?.ToString()?.Trim() ?? string.Empty;
                    rowElement.Add(new XElement(col.ColumnName, value));
                }
                rootElement.Add(rowElement);
            }

            return xdoc;
        }

        /// <summary>
        /// Convierte un DataTable a una clase que pueda mapear
        /// </summary>
        /// <typeparam name="T">Clase u Objeto al cual se mapearan los datos</typeparam>
        /// <param name="dt">DataTable de origen de datos</param>
        /// <returns></returns>
        public static IEnumerable<T> ConvertTo<T>(this DataTable dt)
    where T : class, new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(p => p.CanWrite)
                                      .ToList();

            foreach (DataRow row in dt.Rows)
            {
                var instance = new T();
                foreach (var property in properties)
                {
                    if (dt.Columns.Contains(property.Name))
                    {
                        var value = row[property.Name];
                        if (value != DBNull.Value)
                        {
                            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                            if (property.PropertyType.IsEnum)
                            {
                                if (Enum.IsDefined(targetType, value.ToString()))
                                {
                                    var eValue = Enum.ToObject(targetType, value.ToString());
                                    property.SetValue(instance, eValue);
                                }
                            }
                            else
                            {
                                property.SetValue(instance, Convert.ChangeType(value, targetType));
                            }
                        }
                    }
                }
                list.Add(instance);
            }

            return list;
        }

        /// <summary>
        /// Convierte un objeto de tipo <typeparamref name="TSource"/> en un <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="TSource">El tipo del objeto que se convertirá en un DataTable.</typeparam>
        /// <param name="source">El objeto fuente que se convertirá en un DataTable.</param>
        /// <returns>Un <see cref="DataTable"/> que representa los datos del objeto fuente.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si el parámetro <paramref name="source"/> es nulo.</exception>
        public static DataTable ConvertToDataTable<TSource>(this TSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            DataTable dt = new DataTable();
            Type type = typeof(TSource);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Crear columnas basadas en las propiedades del objeto
            foreach (var property in properties)
            {
                Type propertyType = property.PropertyType;

                // Manejar tipos Nullable
                if (Nullable.GetUnderlyingType(propertyType) != null)
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                dt.Columns.Add(property.Name, propertyType);
            }

            // Crear una fila con los valores del objeto
            var row = dt.NewRow();
            foreach (var property in properties)
            {
                object value = property.GetValue(source, null);
                row[property.Name] = value ?? DBNull.Value;
            }

            dt.Rows.Add(row);

            return dt;
        }
        #endregion Methods
    }
}
