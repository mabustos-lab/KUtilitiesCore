using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    /// <summary>
    /// Proporciona métodos de extensión para la clase <see cref="DataTable"/>.
    /// </summary>
    public static class DataTableExt
    {

        /// <summary>
        /// Clave usada para almacenar la descripción de una columna.
        /// </summary>
        private const string ColumnDescriptionKey = "ColumnDescription";

        /// <summary>
        /// Clave usada para establecer el Formato definir formatos (ej. "C2" para moneda o
        /// "yyyy-MM-dd" para fechas) en el DataTable y se aplicarán automáticamente en el Excel.
        /// </summary>
        private const string DisplayFormatKey = "DisplayFormat";

        /// <summary>
        /// Clave usada para marcar columnas que deben excluirse en ciertos procesos.
        /// </summary>
        private const string ExcludeColumnKey = "ExcludeColumn";
        /// <summary>
        /// Clave usada para almacenar el ancho de columna preferido en Excel.
        /// </summary>
        private const string ColumnWidthKey = "ColumnWidth";
        /// <summary>
        /// Establece el ancho preferido para una columna en Excel.
        /// </summary>
        /// <param name="column">Columna a configurar.</param>
        /// <param name="width">Ancho en unidades de Excel (0 para autoajuste).</param>
        public static void SetColumnWidth(this DataColumn column, double width)
        {
            if (column == null || width < 0)
                return;

            column.ExtendedProperties[ColumnWidthKey] = width;
        }

        /// <summary>
        /// Obtiene el ancho configurado para una columna en Excel.
        /// </summary>
        /// <param name="column">Columna a consultar.</param>
        /// <returns>Ancho configurado o 0 si no está configurado.</returns>
        public static double GetColumnWidth(this DataColumn column)
        {
            if (column == null)
                return 0;

            if (column.ExtendedProperties.ContainsKey(ColumnWidthKey))
            {
                if (column.ExtendedProperties[ColumnWidthKey] is double width)
                    return width;
            }

            return 0;
        }
        /// <summary>
        /// Agrega una nueva columna al <see cref="DataTable"/> con el tipo especificado.
        /// </summary>
        /// <typeparam name="TType">Tipo de datos de la columna.</typeparam>
        /// <param name="dt"><see cref="DataTable"/> al que se agregará la columna.</param>
        /// <param name="fieldName">Nombre del campo de la columna.</param>
        /// <param name="captionColumn">Texto a mostrar como cabecera (opcional).</param>
        /// <param name="description">Descripción de la columna (opcional).</param>
        /// <param name="excludeColumnFlag">
        /// Indica si la columna debe excluirse en ciertos procesos (opcional).
        /// </param>
        /// <param name="displayFormat">
        /// Establece el formato (ej. "C2" para moneda o "yyyy-MM-dd" para fechas) se aplicarán
        /// automáticamente en el Excel.
        /// </param>
        /// <returns>La columna recién creada o <c>null</c> si <paramref name="dt"/> es <c>null</c>.</returns>
        public static DataColumn AddColumn<TType>(
            this DataTable dt,
            string fieldName,
            string captionColumn = "",
            string description = "",
            bool excludeColumnFlag = false,
            string displayFormat = "")
        {
            return dt.AddColumn(typeof(TType), fieldName, captionColumn, description, excludeColumnFlag, displayFormat);
        }

        /// <summary>
        /// Agrega una nueva columna al <see cref="DataTable"/> con el tipo especificado.
        /// </summary>
        /// <typeparam name="TType">Tipo de datos de la columna.</typeparam>
        /// <param name="dt"><see cref="DataTable"/> al que se agregará la columna.</param>
        /// <param name="dataType">Tipo de datos de la columna.</param>
        /// <param name="fieldName">Nombre del campo de la columna.</param>
        /// <param name="captionColumn">Texto a mostrar como cabecera (opcional).</param>
        /// <param name="description">Descripción de la columna (opcional).</param>
        /// <param name="excludeColumnFlag">
        /// Indica si la columna debe excluirse en ciertos procesos (opcional).
        /// </param>
        /// <param name="displayFormat">
        /// Establece el formato (ej. "C2" para moneda o "yyyy-MM-dd" para fechas) se aplicarán
        /// automáticamente en el Excel.
        /// </param>
        /// <returns>La columna recién creada o <c>null</c> si <paramref name="dt"/> es <c>null</c>.</returns>
        public static DataColumn AddColumn(
            this DataTable dt,
            Type dataType,
            string fieldName,
            string captionColumn = "",
            string description = "",
            bool excludeColumnFlag = false,
            string displayFormat = "")
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));

            Type underlyingType = Nullable.GetUnderlyingType(dataType) ?? dataType;
            bool allowDBNull = Nullable.GetUnderlyingType(dataType) != null || !dataType.IsValueType;

            DataColumn dataColumn = dt.Columns.Add(fieldName, underlyingType);
            dataColumn.AllowDBNull = allowDBNull;

            if (!string.IsNullOrEmpty(captionColumn))
                dataColumn.Caption = captionColumn;

            if (excludeColumnFlag)
                dataColumn.SetExcluded(true);

            if (!string.IsNullOrEmpty(description))
                dataColumn.SetDescription(description);

            if (!string.IsNullOrEmpty(displayFormat))
                dataColumn.SetDisplayFormat(displayFormat);

            return dataColumn;
        }

        /// <summary>
        /// Obtiene la descripción almacenada para una columna específica.
        /// </summary>
        /// <param name="dt"><see cref="DataTable"/> que contiene la columna.</param>
        /// <param name="columnName">Nombre de la columna.</param>
        /// <returns>La descripción de la columna si existe; de lo contrario, <see cref="string.Empty"/>.</returns>
        public static string GetDescription(this DataTable dt, string columnName)
        {
            if (dt == null || !dt.Columns.Contains(columnName))
                return string.Empty;

            var column = dt.Columns[columnName];

            if (column.ExtendedProperties.ContainsKey(ColumnDescriptionKey))
            {
                // Obtiene el valor y lo convierte a string, manejando null
                var description = column.ExtendedProperties[ColumnDescriptionKey];
                return description?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene la descripción almacenada para una columna específica.
        /// </summary>
        /// <param name="column">La columna de la cual obtener la descripción.</param>
        /// <returns>La descripción de la columna si existe; de lo contrario, <see cref="string.Empty"/>.</returns>
        public static string GetDescription(this DataColumn column)
        {
            if (column == null)
                return string.Empty;

            if (column.ExtendedProperties.ContainsKey(ColumnDescriptionKey))
            {
                var description = column.ExtendedProperties[ColumnDescriptionKey];
                return description?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el texto que etablece el formato para Excel
        /// </summary>
        public static string GetDisplayFormat(this DataColumn column)
        {
            return column.ExtendedProperties.ContainsKey(DisplayFormatKey)
                ? column.ExtendedProperties[DisplayFormatKey]?.ToString() ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        /// Obtiene el valor que indica si la columna tiene la bandera de excluido para ser exportado.
        /// </summary>
        public static bool IsExcluded(this DataColumn column)
        {
            if (column == null) return false;

            return column.ExtendedProperties.ContainsKey(ExcludeColumnKey) &&
                   column.ExtendedProperties[ExcludeColumnKey] is bool excludeFlag &&
                   excludeFlag;
        }
        /// <summary>
        /// Agrega o elimina una descripción para una columna existente.
        /// </summary>
        public static void SetDescription(this DataColumn column, string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                if (column.ExtendedProperties.ContainsKey(ColumnDescriptionKey))
                    column.ExtendedProperties.Remove(ColumnDescriptionKey);
            }
            else
            {
                column.ExtendedProperties[ColumnDescriptionKey] = description;
            }
        }

        /// <summary>
        /// Formato de Visualización (Excel)
        /// </summary>
        /// <param name="column">Columna a la cul se establece el formato</param>
        /// <param name="format">
        /// Establece el formato (ej. "C2" para moneda o "yyyy-MM-dd" para fechas) se aplicarán
        /// automáticamente en el Excel.
        /// </param>
        public static void SetDisplayFormat(this DataColumn column, string format)
        {
            column.ExtendedProperties[DisplayFormatKey] = format;
        }

        /// <summary>
        /// Establece una bandera que esa columna no va a ser exportada
        /// </summary>
        public static void SetExcluded(this DataColumn column, bool exclude)
        {
            if (column == null) return;

            if (exclude)
                column.ExtendedProperties[ExcludeColumnKey] = true;
            else if (column.ExtendedProperties.ContainsKey(ExcludeColumnKey))
                column.ExtendedProperties.Remove(ExcludeColumnKey);
        }

    }
}