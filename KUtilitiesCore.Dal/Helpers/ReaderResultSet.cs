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
        private readonly List<object> _results;
       readonly Dictionary<string, object> _paramsUsed;

        public ReaderResultSet()
        {
            _results = [];
            _paramsUsed = [];
        }

        public bool HasResultsets => _results.Count > 0;
        public int ResultSetCount => _results.Count;

        public IReadOnlyDictionary<string, object> ParamsUsed => _paramsUsed;

        public void AddResult(object result)
        {
            _results.Add(result);
        }

        public IEnumerable<TResult> GetResult<TResult>(int index = 0) where TResult : class, new()
        {
            ValidateIndex(index);
            
            var result = _results[index];

            if (result is IEnumerable<TResult> typedResult)
                return typedResult;

            if (result is DataTable dataTable)
                return DataTableToEnumerable<TResult>(dataTable);

            throw new InvalidCastException($"No se puede convertir el resultado en el índice {index} al tipo {typeof(TResult).Name}.");
        }
        internal void SetParams(IDaoParameterCollection parameters = null)
        {
            if (parameters!=null && parameters.Count > 0)
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

        private IEnumerable<TResult> DataTableToEnumerable<TResult>(DataTable dataTable) where TResult : class, new()
        {
            var properties = GetProperties(typeof(TResult), dataTable);

            foreach (DataRow row in dataTable.Rows)
            {
                var item = new TResult();
                foreach (var prop in properties)
                {
                    if (!row.IsNull(prop.Name))
                    {
                        var value = row[prop.Name];
                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
                yield return item;
            }
        }

        private static PropertyInfo[] GetProperties(Type type, DataTable dataTable)
        {
            var columnNames = string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).OrderBy(c => c));
            var cacheKey = new Tuple<Type, string>(type, columnNames);

            return _propertyCache.GetOrAdd(cacheKey, key =>
            {
                var t = ((Tuple<Type, string>)key).Item1;
                return t.GetProperties()
                    .Where(p => p.CanWrite && dataTable.Columns.Contains(p.Name))
                    .ToArray();
            });
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