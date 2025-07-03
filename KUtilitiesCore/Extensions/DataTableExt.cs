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
        /// Mapea las filas de un <see cref="DataTable"/> a una colección de objetos del tipo especificado.
        /// </summary>
        /// <typeparam name="T">El tipo de objeto al que se mapearán las filas. Debe ser una clase con un constructor sin parámetros.</typeparam>
        /// <param name="dataTable">El <see cref="DataTable"/> que contiene los datos a mapear.</param>
        /// <returns>Una colección <see cref="IEnumerable{T}"/> de objetos mapeados.</returns>
        /// <remarks>
        /// El mapeo se basa en la correspondencia de nombres entre las columnas del <see cref="DataTable"/> y las propiedades públicas de instancia de la clase <typeparamref name="T"/>.
        /// La comparación de nombres no distingue entre mayúsculas y minúsculas.
        /// Las propiedades deben tener un setter público.
        /// Se utiliza un <see cref="ITypeConverterProvider"/> para realizar la conversión de tipos entre los valores de las celdas y los tipos de las propiedades.
        /// </remarks>
        public static IEnumerable<T> MapTo<T>(this DataTable dataTable) where T : class, new()
        {
            // Cache de propiedades del tipo T para optimizar el acceso por reflexión.
            // Se incluyen solo propiedades públicas de instancia que tienen un setter.
            // El diccionario usa comparación ordinal ignorando mayúsculas/minúsculas para los nombres de propiedad.
            var propertyCache = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanWrite)
                                        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dataTable.Rows)
            {
                yield return CreateEntity<T>(row, propertyCache);
            }
        }

        /// <summary>
        /// Convierte una colección de objetos en un <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="TSource">El tipo de los elementos en la colección de origen.</typeparam>
        /// <param name="source">La colección de elementos a convertir.</param>
        /// <param name="onRowAdded">
        /// Un delegado opcional <see cref="Action{DataRow, TSource}"/> que se invoca después de que cada objeto <typeparamref name="TSource"/>
        /// se convierte y agrega como <see cref="DataRow"/> al <see cref="DataTable"/>.
        /// Permite realizar acciones personalizadas en la fila recién agregada.
        /// </param>
        /// <returns>Un <see cref="DataTable"/> que representa la estructura y los datos de la colección de origen.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="source"/> es <c>null</c>.</exception>
        /// <remarks>
        /// Las columnas del <see cref="DataTable"/> se crean a partir de las propiedades públicas de instancia del tipo <typeparamref name="TSource"/>.
        /// El nombre de cada propiedad se usa como nombre de columna.
        /// El tipo de dato de la columna se determina a partir del tipo de la propiedad, considerando tipos anulables (<see cref="Nullable{T}"/>).
        /// </remarks>
        public static DataTable ToDataTable<TSource>(this IEnumerable<TSource> source,
            Action<DataRow, TSource>? onRowAdded = null) where TSource : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            // Primero, crea la estructura del DataTable basada en las propiedades de TSource.
            var dataTable = CreateDataTableStructure<TSource>();
            // Luego, llena el DataTable con los datos de la colección.
            PopulateDataTableRows(dataTable, source, onRowAdded);

            return dataTable;
        }

        /// <summary>
        /// Convierte un <see cref="DataTable"/> en una cadena de texto con formato estructurado, usando separadores personalizables.
        /// </summary>
        /// <param name="dataTable">El <see cref="DataTable"/> a convertir.</param>
        /// <param name="includeHeader">
        /// Un valor booleano que indica si se debe incluir una fila de encabezado con los nombres de las columnas.
        /// El valor predeterminado es <c>true</c>.
        /// </param>
        /// <param name="separator">
        /// La cadena que se utilizará como separador entre los valores de las celdas en cada fila y entre los nombres de las columnas en el encabezado.
        /// El valor predeterminado es un carácter de tabulación ("\t").
        /// </param>
        /// <returns>Una cadena que representa el contenido del <see cref="DataTable"/> formateado como texto.</returns>
        /// <remarks>
        /// Si un valor de celda contiene el carácter separador, el valor se encapsulará entre comillas dobles (").
        /// </remarks>
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
        /// Serializa un <see cref="DataTable"/> a un documento XML (<see cref="XDocument"/>).
        /// </summary>
        /// <param name="dataTable">El <see cref="DataTable"/> a serializar.</param>
        /// <param name="rootName">
        /// El nombre para el elemento raíz del documento XML.
        /// Si es nulo, está vacío o consiste solo en espacios en blanco, se utilizará "Data" como nombre predeterminado.
        /// </param>
        /// <returns>Un <see cref="XDocument"/> que representa el contenido del <see cref="DataTable"/>.</returns>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="dataTable"/> es <c>null</c>.</exception>
        /// <remarks>
        /// El documento XML tendrá una declaración XML `<?xml version="1.0" encoding="utf-8"?>`.
        /// Cada <see cref="DataRow"/> en el <see cref="DataTable"/> se convierte en un elemento XML. El nombre de este elemento
        /// se toma de <see cref="DataTable.TableName"/>; si <see cref="DataTable.TableName"/> está vacío o es nulo, se usa "Row".
        /// Cada <see cref="DataColumn"/> en una fila se convierte en un elemento XML secundario, donde el nombre del elemento es
        /// el <see cref="DataColumn.ColumnName"/> (sanitizado para ser un nombre XML válido) y el contenido es el valor de la celda como cadena.
        /// Los nombres de columna se sanitizan utilizando <see cref="SanitizeXmlName"/> para asegurar que sean nombres XML válidos.
        /// </remarks>
        public static XDocument ToXml(this DataTable dataTable, string rootName = "Data")
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            // Si rootName es nulo, vacío o espacios en blanco, se usa "Data" por defecto.
            if (string.IsNullOrWhiteSpace(rootName)) rootName = "Data";

            var rootElement = new XElement(rootName);
            var xmlDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);

            foreach (DataRow row in dataTable.Rows)
            {
                // Usa el nombre de la tabla si está disponible, sino "Row".
                var rowElement = new XElement(dataTable.TableName.DefaultIfEmpty("Row"));
                foreach (DataColumn column in dataTable.Columns)
                {
                    // Convierte el valor de la celda a cadena y elimina espacios al inicio/final.
                    var cellValue = (row[column]?.ToString() ?? string.Empty).Trim();
                    // Sanitiza el nombre de la columna para que sea un nombre XML válido.
                    rowElement.Add(new XElement(column.ColumnName.SanitizeXmlName(), cellValue));
                }
                rootElement.Add(rowElement);
            }

            return xmlDocument;
        }

        /// <summary>
        /// Convierte un valor a un tipo de destino especificado, manejando enumeraciones y conversiones estándar.
        /// </summary>
        /// <param name="value">El valor a convertir. Puede ser <c>null</c>.</param>
        /// <param name="targetType">El tipo al que se debe convertir el valor.</param>
        /// <param name="provider">
        /// El proveedor de convertidores de tipo, usado actualmente de forma implícita por <see cref="CreateEntity{T}"/>.
        /// Este parámetro no se usa directamente en la implementación actual de `ConvertValue` pero está presente para posible extensibilidad futura.
        /// </param>
        /// <returns>El valor convertido al tipo <paramref name="targetType"/>.</returns>
        /// <remarks>
        /// Si <paramref name="targetType"/> es un tipo de enumeración, el valor se interpreta como el nombre de una constante de la enumeración.
        /// Para otros tipos, se utiliza <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> con <see cref="CultureInfo.InvariantCulture"/>
        /// para realizar la conversión.
        /// </remarks>
        private static object ConvertValue(object value, Type targetType, ITypeConverterProvider provider)
        {
            // Si el tipo de destino es una enumeración, intenta analizar el valor como una cadena.
            // Esto asume que 'value.ToString()' devolverá el nombre de un miembro de la enumeración.
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value?.ToString() ?? string.Empty);
            }
            // Para otros tipos, usa Convert.ChangeType con la cultura invariante para consistencia.
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Crea una nueva <see cref="DataRow"/> para el <see cref="DataTable"/> especificado y la llena con los valores de las propiedades del objeto <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="TSource">El tipo del objeto de origen.</typeparam>
        /// <param name="dataTable">El <see cref="DataTable"/> al que se agregará la nueva fila.</param>
        /// <param name="item">El objeto de origen cuyos valores de propiedad se usarán para llenar la fila.</param>
        /// <returns>La <see cref="DataRow"/> recién creada y agregada al <paramref name="dataTable"/>.</returns>
        /// <remarks>
        /// Se crea una nueva fila utilizando <see cref="DataTable.NewRow()"/>.
        /// Se iteran las propiedades públicas de instancia del tipo <typeparamref name="TSource"/>.
        /// Para cada propiedad, su valor se asigna a la columna correspondiente (por nombre) en la <see cref="DataRow"/>.
        /// Si el valor de una propiedad es <c>null</c>, se asigna <see cref="DBNull.Value"/> a la celda.
        /// Finalmente, la fila se agrega a la colección de filas del <paramref name="dataTable"/>.
        /// </remarks>
        private static DataRow CreateDataRow<TSource>(DataTable dataTable, TSource item)
        {
            var row = dataTable.NewRow();
            // Obtiene todas las propiedades públicas de instancia del tipo TSource.
            var properties = typeof(TSource).GetRuntimeProperties();

            foreach (var property in properties)
            {
                // Asigna el valor de la propiedad a la columna con el mismo nombre.
                // Si el valor es null, se usa DBNull.Value.
                row[property.Name] = property.GetValue(item) ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Crea la estructura de un <see cref="DataTable"/> (columnas y tipos) basada en las propiedades públicas de instancia del tipo <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">El tipo de objeto a partir del cual se define la estructura del <see cref="DataTable"/>.</typeparam>
        /// <returns>Un <see cref="DataTable"/> con las columnas definidas, pero sin filas.</returns>
        /// <remarks>
        /// Para cada propiedad pública de instancia de <typeparamref name="TSource"/>:
        /// - Se crea una <see cref="DataColumn"/>.
        /// - El nombre de la columna es el nombre de la propiedad.
        /// - El tipo de datos de la columna (<see cref="DataColumn.DataType"/>) se determina a partir del tipo de la propiedad.
        ///   Se utiliza <see cref="GetUnderlyingType"/> para manejar correctamente los tipos <see cref="Nullable{T}"/>,
        ///   estableciendo el tipo de la columna al tipo subyacente no anulable.
        /// </remarks>
        public static DataTable CreateDataTableStructure<TSource>()
        {
            var dataTable = new DataTable();
            var sourceType = typeof(TSource);
            // Obtiene las propiedades públicas de instancia del tipo TSource.
            var properties = sourceType.GetRuntimeProperties().ToList();

            foreach (var property in properties)
            {
                // Determina el tipo de la columna, manejando tipos Nullable<T>.
                var columnType = GetUnderlyingType(property.PropertyType);
                dataTable.Columns.Add(property.Name, columnType);
            }

            return dataTable;
        }

        /// <summary>
        /// Crea una nueva instancia de tipo <typeparamref name="T"/> y la llena con datos de una <see cref="DataRow"/>.
        /// </summary>
        /// <typeparam name="T">El tipo de entidad a crear. Debe ser una clase con un constructor sin parámetros.</typeparam>
        /// <param name="row">La <see cref="DataRow"/> que contiene los datos para la entidad.</param>
        /// <param name="properties">
        /// Un diccionario de solo lectura que mapea nombres de propiedades (ignorando mayúsculas/minúsculas) a sus objetos <see cref="PropertyInfo"/>.
        /// Este caché se utiliza para optimizar el acceso a las propiedades de la entidad.
        /// </param>
        /// <returns>Una nueva instancia de <typeparamref name="T"/> con sus propiedades llenas desde la <paramref name="row"/>.</returns>
        /// <remarks>
        /// Para cada propiedad en <paramref name="properties"/>:
        /// - Se verifica si existe una columna en <paramref name="row"/> con el mismo nombre (comparación sensible a mayúsculas/minúsculas por defecto de DataTable).
        /// - Si la columna existe y su valor no es <see cref="DBNull.Value"/>:
        ///     - Se obtiene el tipo subyacente de la propiedad (para manejar <see cref="Nullable{T}"/>).
        ///     - Se resuelve un <see cref="ITypeConverter"/> para el tipo de destino utilizando <see cref="Data.Converter.TypeConverterFactory.Provider"/>.
        ///     - El valor de la celda (convertido a cadena) se pasa al método <see cref="ITypeConverter.TryConvert"/> del convertidor.
        ///     - El valor convertido se asigna a la propiedad de la entidad.
        /// </remarks>
        private static T CreateEntity<T>(DataRow row, IReadOnlyDictionary<string, PropertyInfo> properties) where T : new()
        {
            ITypeConverterProvider provider = Data.Converter.TypeConverterFactory.Provider;
            var entity = new T();
            foreach (var property in properties.Values)
            {
                // Verifica si la DataRow contiene una columna con el nombre de la propiedad.
                if (!row.Table.Columns.Contains(property.Name)) continue;

                var value = row[property.Name];
                // Si el valor es DBNull, no se asigna nada a la propiedad (conserva su valor predeterminado).
                if (value == DBNull.Value) continue;

                // Obtiene el tipo de destino real de la propiedad (maneja Nullable<T>).
                var targetType = GetUnderlyingType(property.PropertyType);
                // Resuelve el convertidor de tipo adecuado para el tipo de destino.
                ITypeConverter typeConverter= provider.Resolve(targetType);
                // Intenta convertir el valor de la celda (como cadena) al tipo de destino.
                var convertedValue = typeConverter.TryConvert(value?.ToString()??string.Empty);
                property.SetValue(entity, convertedValue);
            }
            return entity;
        }

        /// <summary>
        /// Devuelve la cadena <paramref name="defaultValue"/> si <paramref name="value"/> es nula, vacía o consiste solo en espacios en blanco;
        /// de lo contrario, devuelve <paramref name="value"/>.
        /// </summary>
        /// <param name="value">La cadena a verificar.</param>
        /// <param name="defaultValue">El valor predeterminado a devolver si <paramref name="value"/> está vacía o es nula.</param>
        /// <returns>
        /// <paramref name="defaultValue"/> si <paramref name="value"/> es nula, vacía o espacios en blanco;
        /// de lo contrario, <paramref name="value"/>.
        /// </returns>
        private static string DefaultIfEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        /// <summary>
        /// Formatea los nombres de las columnas de un <see cref="DataTable"/> en una sola cadena, separada por el <paramref name="separator"/> especificado.
        /// </summary>
        /// <param name="columns">Una colección de objetos <see cref="DataColumn"/>.</param>
        /// <param name="separator">La cadena utilizada para separar los nombres de las columnas.</param>
        /// <returns>Una cadena que contiene los nombres de las columnas concatenados, separados por <paramref name="separator"/>.</returns>
        private static string FormatHeader(IEnumerable<DataColumn> columns, string separator)
        {
            // Une los nombres de las columnas usando el separador.
            return string.Join(separator, columns.Select(c => c.ColumnName));
        }

        /// <summary>
        /// Formatea los valores de una <see cref="DataRow"/> en una sola cadena, utilizando un separador y encapsulando valores si es necesario.
        /// </summary>
        /// <param name="row">La <see cref="DataRow"/> cuyos valores se van a formatear.</param>
        /// <param name="columns">La colección de <see cref="DataColumn"/> que define el orden y las columnas a incluir.</param>
        /// <param name="separator">La cadena utilizada para separar los valores formateados de las celdas.</param>
        /// <returns>Una cadena que representa la fila formateada.</returns>
        /// <remarks>
        /// Para cada columna especificada en <paramref name="columns"/>:
        /// - Se obtiene el valor de la celda correspondiente de la <paramref name="row"/> y se convierte a cadena (si es <c>null</c>, se usa una cadena vacía).
        /// - Si el valor de la celda contiene el <paramref name="separator"/>, el valor se encierra entre comillas dobles (").
        /// - Los valores formateados de todas las columnas se unen mediante el <paramref name="separator"/>.
        /// </remarks>
        private static string FormatRow(DataRow row, IEnumerable<DataColumn> columns, string separator)
        {
            var formattedValues = columns.Select(column =>
            {
                var value = row[column]?.ToString() ?? string.Empty;
                // Si el valor contiene el separador, lo encierra entre comillas.
                // Esto es una forma simple de CSV escaping.
                return value.Contains(separator) ? $"\"{value}\"" : value;
            });

            return string.Join(separator, formattedValues);
        }

        /// <summary>
        /// Obtiene el tipo subyacente de un tipo dado. Si el tipo es <see cref="Nullable{T}"/>, devuelve el tipo argumento genérico;
        /// de lo contrario, devuelve el tipo original.
        /// </summary>
        /// <param name="type">El tipo a examinar.</param>
        /// <returns>
        /// El tipo de argumento genérico si <paramref name="type"/> es <see cref="Nullable{T}"/>;
        /// de lo contrario, <paramref name="type"/>.
        /// </returns>
        private static Type GetUnderlyingType(Type type)
        {
            // Si el tipo es Nullable<T>, devuelve T. Sino, devuelve el tipo original.
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Llena un <see cref="DataTable"/> existente con datos de una colección de objetos <typeparamref name="TSource"/>.
        /// </summary>
        /// <typeparam name="TSource">El tipo de los elementos en la colección de origen. Debe ser una clase.</typeparam>
        /// <param name="dataTable">El <see cref="DataTable"/> que se va a llenar.</param>
        /// <param name="source">La colección de objetos <typeparamref name="TSource"/> que proporcionan los datos.</param>
        /// <param name="onRowAdded">
        /// Un delegado opcional <see cref="Action{DataRow, TSource}"/> que se invoca después de que cada objeto <typeparamref name="TSource"/>
        /// se convierte y agrega como <see cref="DataRow"/> al <paramref name="dataTable"/>.
        /// </param>
        /// <remarks>
        /// Para cada elemento en la colección <paramref name="source"/>:
        /// - Se llama a <see cref="CreateDataRow{TSource}"/> para convertir el elemento en una <see cref="DataRow"/> y agregarla al <paramref name="dataTable"/>.
        /// - Si se proporciona el delegado <paramref name="onRowAdded"/>, se invoca con la fila recién creada y el elemento de origen.
        /// </remarks>
        private static void PopulateDataTableRows<TSource>(DataTable dataTable,
            IEnumerable<TSource> source, Action<DataRow, TSource>? onRowAdded) where TSource : class
        {
            foreach (var item in source)
            {
                // Crea una DataRow a partir del item y la agrega al DataTable.
                var row = CreateDataRow(dataTable, item);
                // Si hay una acción post-adición, la invoca.
                onRowAdded?.Invoke(row, item);
            }
        }

        /// <summary>
        /// Sanitiza una cadena para que sea un nombre válido para un elemento o atributo XML.
        /// </summary>
        /// <param name="name">La cadena a sanitizar.</param>
        /// <returns>
        /// Una cadena que contiene solo caracteres alfanuméricos y guiones bajos ('_') del nombre original.
        /// Los caracteres inválidos son eliminados.
        /// </returns>
        /// <remarks>
        /// Esta es una sanitización básica. Para una conformidad XML más estricta, se podrían necesitar reglas adicionales
        /// (por ejemplo, el primer carácter no puede ser un número o '_', aunque <see cref="XName"/> podría manejar esto).
        /// </remarks>
        private static string SanitizeXmlName(this string name)
        {
            // Crea una nueva cadena conservando solo letras, dígitos o guiones bajos.
            return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

    }
}