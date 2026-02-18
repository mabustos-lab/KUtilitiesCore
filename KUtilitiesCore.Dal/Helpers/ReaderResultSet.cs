#nullable enable
using KUtilitiesCore.Extensions;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace KUtilitiesCore.Dal.Helpers
{
    internal class ReaderResultSet : IReaderResultSet
    {
        private static readonly ConcurrentDictionary<object, PropertyInfo[]> _propertyCache = new();
        internal readonly List<object> _results;
        readonly Dictionary<string, object> _paramsUsed;
        private bool _useDefaultDataTable;

        public ReaderResultSet()
        {
            _results = [];
            _paramsUsed = [];
            _useDefaultDataTable = false;
        }

        public bool HasResultsets => _results.Count > 0;
        public int ResultSetCount => _results.Count;

        public IReadOnlyDictionary<string, object> ParamsUsed => _paramsUsed;

        internal void AddResult(object result)
        {
            _results.Add(result);
        }

        internal void AddResults(IEnumerable<object> results)
        {
            _results.AddRange(results);
        }

        internal void Load(IDataReader reader, Queue<IMappingStrategy> strategies, bool useDefaultDataTable)
        {
            _useDefaultDataTable = useDefaultDataTable;

            IMappingStrategy currentStrategy = null;
            if (strategies.Count > 0)
            {
                currentStrategy = strategies.Dequeue();
            }
            else if (_useDefaultDataTable)
            {
                currentStrategy = new DataTableMappingStrategy();
            }

            if (currentStrategy != null)

            {

                DataTable currentResultSetTable = CreateDataTableFromReader(reader);

                _results.Add(currentStrategy.Map(currentResultSetTable));

            }
            else
            {
                // Si no se define ninguna estrategia y no se establece ningún valor predeterminado, omita este conjunto de resultados.
                // Aún necesitamos consumir el conjunto de resultados actual para avanzar en el lector.
                CreateDataTableFromReader(reader); // Consumir los datos
            }
        }
        public IEnumerable<TResult> GetResult<TResult>(int index = 0) where TResult : class, new()
        {
            ValidateIndex(index);

            var result = _results[index];

            if (result is IEnumerable<TResult> typedResult)
                return typedResult;

            if (result is DataTable dataTable)
                return DataTableToEnumerable<TResult>(dataTable, new TranslateOptions()); // Opciones predeterminadas si no se proporcionan

            throw new InvalidCastException($"No se puede convertir el resultado en el índice {index} al tipo {typeof(TResult).Name}.");
        }
        internal void SetParams(IDaoParameterCollection parameters = null)
        {
            if (parameters != null && parameters.Count > 0)
            {
                parameters.ForEach(x => _paramsUsed.Add(x.ParameterName, x.Value));
            }
        }
        public DataTable GetDataTable(int index = 0)
        {
            ValidateIndex(index);

            var result = _results[index];

            if (result is DataTable dataTable)
                return dataTable;

            if (result is IEnumerable enumerable)
                return EnumerableToDataTable(enumerable);

            throw new InvalidCastException($"No se puede convertir el resultado en el índice {index} a DataTable.");
        }

        public DataTable[] GetAllDataTables()
        {
            return _results.Select((result, index) =>
            {
                try
                {
                    return GetDataTable(index);
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException($"No se puede convertir el resultado en el índice {index} a DataTable.");
                }
            }).ToArray();
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _results.Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"El índice {index} está fuera del rango. Hay {_results.Count} conjuntos de resultados disponibles.");
        }

        internal static IEnumerable<TResult> DataTableToEnumerable<TResult>(DataTable dataTable, TranslateOptions options) where TResult : class, new()
        {
            var properties = GetProperties(typeof(TResult), dataTable, options);

            foreach (DataRow row in dataTable.Rows)
            {
                var item = new TResult();
                foreach (var prop in properties)
                {
                    if (dataTable.Columns.Contains(prop.Name) && !row.IsNull(prop.Name))
                    {
                        var value = row[prop.Name];
                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
                yield return item;
            }
        }

        private static PropertyInfo[] GetProperties(Type type, DataTable dataTable, TranslateOptions options)

        {

            var t = type;

            var dataTableColumnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();



            var props = t.GetProperties()

                .Where(p => p.CanWrite && dataTableColumnNames.Contains(p.Name));



            if (options.StrictMapping)
            {
                var missingProps = props.Where(p => !dataTableColumnNames.Contains(p.Name)).ToList();
                if (missingProps.Any())
                {
                    throw new InvalidOperationException($"Strict mapping failed: Properties {string.Join(", ", missingProps.Select(p => p.Name))} not found in DataTable.");
                }
            }



            return props.ToArray();

        }
        private static DataTable CreateDataTableFromReader(IDataReader reader)
        {
            var dataTable = new DataTable();

            // Manually get schema and load data to prevent reader from closing
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            object[] values = new object[reader.FieldCount];
            while (reader.Read())
            {
                reader.GetValues(values);
                DataRow newRow = dataTable.NewRow();
                for (int i = 0; i < values.Length; i++)
                {
                    newRow[i] = values[i];
                }
                dataTable.Rows.Add(newRow);
            }
            return dataTable;
        }
        private DataTable EnumerableToDataTable(IEnumerable enumerable)
        {
            var dataTable = new DataTable();
            var firstItem = enumerable.Cast<object>().FirstOrDefault();

            if (firstItem == null)
                return dataTable;

            // Crear columnas basadas en las propiedades del primer elemento
            var properties = firstItem.GetType().GetProperties();
            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Llenar el DataTable con datos
            foreach (var item in enumerable)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item) ?? DBNull.Value;
                    row[prop.Name] = value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}