using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Helpers
{
    public static class IDataReaderExt
    {
        /// <summary>
        /// Traduce un IDataReader a una colección de objetos del tipo especificado.
        /// </summary>
        /// <typeparam name="TResult">El tipo de objetos a crear</typeparam>
        /// <param name="reader">El IDataReader origen</param>
        /// <returns>Unenumerable de resultados</returns>
        public static IEnumerable<TResult> Translate<TResult>(this IDataReader reader)
            where TResult : new()
        {
            var columnMeta = GetColumnMetaData(reader);
            var propertyMap = GetPropertyMapping<TResult>(columnMeta);
            var cache = GetPropertyMapCache<TResult>(propertyMap);

            while (reader.Read())
            {
                var newInstance = new TResult();
                AssignValues(newInstance, reader, cache);
                yield return newInstance;
            }
        }

        // Métodos privados optimizados
        private static Dictionary<string, int> GetColumnMetaData(IDataReader reader)
        {
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnMap[reader.GetName(i)] = i;
            }

            return columnMap;
        }

        private static Dictionary<PropertyInfo, int> GetPropertyMapping<TResult>(Dictionary<string, int> columnMap)
        {
            var properties = typeof(TResult).GetProperties()
                .Where(p=>p.CanWrite);
            var propertyMap = new Dictionary<PropertyInfo, int>();

            foreach (var prop in properties)
            {
                if (columnMap.TryGetValue(prop.Name, out int index))
                {
                    propertyMap[prop] = index;
                }
            }

            return propertyMap;
        }

        private static void AssignValues<TResult>(TResult instance, IDataReader reader, Dictionary<PropertyInfo, int> propertyMap)
        {
            foreach (var propertyInfo in propertyMap.Keys)
            {
                var columnIndex = propertyMap[propertyInfo];
                if (reader.IsDBNull(columnIndex)) continue;

                var value = reader.GetValue(columnIndex);
                SetPropertyValue(instance, propertyInfo, value);
            }
        }

        private static void SetPropertyValue<TResult>(TResult instance, PropertyInfo property, object value)
        {
            try
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error al asignar valor a la propiedad {property.Name} en el objeto de tipo {typeof(TResult).Name}: {value?.ToString()}", ex);
            }
        }

        // Cache de mapeos para mejora de rendimiento
        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, int>> PropertyMapCache =
            new ConcurrentDictionary<Type, Dictionary<PropertyInfo, int>>();

        private static Dictionary<PropertyInfo, int> GetPropertyMapCache<TResult>(Dictionary<PropertyInfo, int> map)
        {
            return PropertyMapCache.GetOrAdd(typeof(TResult), _ => map);
        }
    }
}
