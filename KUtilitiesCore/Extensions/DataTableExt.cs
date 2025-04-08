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
        public static XDocument ToXml(this DataTable dt, string rootName)
        {
            var xdoc = new XDocument
            {
                Declaration = new XDeclaration("1.0", "utf-8", "")
            };
            xdoc.Add(new XElement(rootName));
            foreach (DataRow row in dt.Rows)
            {
                var element = new XElement(dt.TableName);
                foreach (DataColumn col in dt.Columns)
                {
                    element.Add(new XElement(col.ColumnName, row[col].ToString().Trim(' ')));
                }
                if (xdoc.Root != null) xdoc.Root.Add(element);
            }

            return xdoc;
        }

        ///// <summary>
        ///// Convierte un DataTable a una clase que pueda mapear
        ///// </summary>
        ///// <typeparam name="T">Clase u Objeto al cual se mapearan los datos</typeparam>
        ///// <param name="dt">DataTable de origen de datos</param>
        ///// <returns></returns>
        //public static IEnumerable<T> ConvertTo<T>(this DataTable dt)
        //    where T : class
        //{
        //    List<T> list = new List<T>();
        //    List<PropertyInfo> piList = typeof(T).GetPropertiesInfo(false, x => x.CanWrite).ToList();

        //    try
        //    {
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            DataRow dr = dt.Rows[i];
        //            T ret = (T)Activator.CreateInstance(typeof(T));
        //            foreach (PropertyInfo pi in piList)
        //            {
        //                if (dt.Columns.Contains(pi.Name))
        //                {
        //                    object cVal = dr[pi.Name];
        //                    if (cVal != DBNull.Value)
        //                    {
        //                        if (Nullable.GetUnderlyingType(pi.PropertyType) != null)
        //                        {
        //                            pi.SetValue(ret, Convert.ChangeType(cVal, Type.GetType(Nullable.GetUnderlyingType(pi.PropertyType).ToString())), null);
        //                        }
        //                        else
        //                        {
        //                            bool success = false;
        //                            if (pi.PropertyType.IsEnum)
        //                            {
        //                                Type eType = pi.PropertyType;
        //                                object val = null;
        //                                int intValue = -1;
        //                                if (int.TryParse(cVal.ToString(), out intValue))
        //                                {
        //                                    success = Enum.IsDefined(eType, intValue);
        //                                    if (success)
        //                                    {
        //                                        val = Enum.ToObject(eType, intValue);
        //                                        pi.SetValue(ret, val, null);
        //                                    }
        //                                }
        //                            }
        //                            if (!success)
        //                                pi.SetValue(ret, Convert.ChangeType(cVal, Type.GetType(pi.PropertyType.ToString())), null);
        //                        }
        //                    }
        //                }
        //            }
        //            list.Add(ret);
        //        }

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return list;
        //}
        #endregion Methods
    }
}
