using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace KUtilitiesCore.Dal.Helpers
{
   public static class IDataReaderExt
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, int>> PropertyMapCache =
            new ConcurrentDictionary<Type, Dictionary<PropertyInfo, int>>();

        private static readonly ConcurrentDictionary<string, Dictionary<string, int>> ColumnMetaCache =
            new ConcurrentDictionary<string, Dictionary<string, int>>();
        internal static IEnumerable<TResult> Translate<TResult>(this IDataReader reader) where TResult : new()
        {
            return Translate<TResult>(reader, new TranslateOptions());
        }
        internal static IEnumerable<TResult> Translate<TResult>(this IDataReader reader, TranslateOptions options) where TResult : new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (options == null)
                throw new ArgumentNullException(nameof(options));
            var cacheKey = $"{reader.GetHashCode()}_{typeof(TResult).FullName}";
            var columnMeta = GetColumnMetaData(reader, cacheKey);
            var propertyMap = GetPropertyMapping<TResult>(columnMeta, options);

            // Validación de mapeo estricto
            if (options.StrictMapping)
            {
                ValidateStrictMapping<TResult>(propertyMap, columnMeta, options);
            }

            // Log de propiedades no mapeadas (útil para debugging)
            LogUnmappedProperties<TResult>(propertyMap, columnMeta);

            while (reader.Read())
            {
                yield return CreateInstance<TResult>(reader, propertyMap, options);
            }
        }

        private static TResult CreateInstance<TResult>(
            IDataReader reader,
            Dictionary<PropertyInfo, int> propertyMap,
            TranslateOptions options) where TResult : new()
        {
            var instance = new TResult();
            int mappedProperties = 0;

            foreach (var propertyItem in propertyMap)
            {
                try
                {
                    if (reader.IsDBNull(propertyItem.Value))
                    {
                        // Para tipos nullable, establecer como null
                        if (IsNullableType(propertyItem.Key.PropertyType))
                        {
                            propertyItem.Key.SetValue(instance, null);
                            mappedProperties++;
                        }
                        continue;
                    }

                    var value = reader.GetValue(propertyItem.Value);
                    var convertedValue = ConvertValue(value, propertyItem.Key.PropertyType);
                    propertyItem.Key.SetValue(instance, convertedValue);
                    mappedProperties++;
                }
                catch (Exception ex)
                {
                    if (options.StrictMapping)
                    {
                        throw new InvalidOperationException(
                            $"Error estricto de mapeo en propiedad {propertyItem.Key.Name} de {typeof(TResult).Name}. " +
                            $"Valor: {reader.GetValue(propertyItem.Value)?.ToString() ?? "null"}", ex);
                    }
                    // En modo no estricto, continuamos sin esta propiedad
                }
            }
            // Advertencia si no se mapeó ninguna propiedad
            if (mappedProperties == 0)
            {
                throw new InvalidOperationException(
                    $"No se pudo mapear ninguna propiedad para el tipo {typeof(TResult).Name}. " +
                    $"Verifique que los nombres de columna coincidan con los nombres de propiedad.");
            }

            return instance;
        }
        private static void ValidateStrictMapping<TResult>(
            Dictionary<PropertyInfo, int> propertyMap,
            Dictionary<string, int> columnMeta,
            TranslateOptions options)
        {
            var missingProperties = new HashSet<string>();

            // Verificar propiedades requeridas explícitas
            foreach (var requiredProp in options.RequiredProperties)
            {
                var property = typeof(TResult).GetProperty(requiredProp,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null && !propertyMap.ContainsKey(property))
                {
                    missingProperties.Add(requiredProp);
                }
            }

            // En modo estricto, todas las propiedades writable deben mapearse
            var allWritableProperties = typeof(TResult).GetProperties()
                .Where(p => p.CanWrite && !IsIgnoredProperty(p))
                .ToArray();

            foreach (var prop in allWritableProperties)
            {
                if (!propertyMap.ContainsKey(prop) && !IsOptionalProperty(prop))
                {
                    missingProperties.Add(prop.Name);
                }
            }

            if (missingProperties.Any())
            {
                throw new InvalidOperationException(
                    $"Mapeo estricto fallido para {typeof(TResult).Name}. " +
                    $"Propiedades no mapeadas: {string.Join(", ", missingProperties)}. " +
                    $"Columnas disponibles: {string.Join(", ", columnMeta.Keys)}");
            }
        }
        private static void LogUnmappedProperties<TResult>(
            Dictionary<PropertyInfo, int> propertyMap,
            Dictionary<string, int> columnMeta)
        {
            var unmappedProperties = typeof(TResult).GetProperties()
                .Where(p => p.CanWrite && !propertyMap.ContainsKey(p))
                .Select(p => p.Name)
                .ToArray();

            var unmappedColumns = columnMeta.Keys
                .Where(col => !propertyMap.Any(p =>
                    string.Equals(p.Key.Name, col, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            // En un entorno real, usaríamos ILogger aquí
            if (unmappedProperties.Any())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ADVERTENCIA] Translate<{typeof(TResult).Name}>: " +
                    $"Propiedades no mapeadas: {string.Join(", ", unmappedProperties)}");
            }

            if (unmappedColumns.Any())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[ADVERTENCIA] Translate<{typeof(TResult).Name}>: " +
                    $"Columnas no utilizadas: {string.Join(", ", unmappedColumns)}");
            }
        }

        private static Dictionary<string, int> GetColumnMetaData(IDataReader reader, string cacheKey)
        {
            return ColumnMetaCache.GetOrAdd(cacheKey, _ =>
            {
                var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    columnMap[columnName] = i;
                }
                return columnMap;
            });
        }

        internal static string[] GetPropertiesRequired<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && !IsIgnoredProperty(p))
                    .Select(x=>x.Name)
                    .ToArray();
        }

        private static Dictionary<PropertyInfo, int> GetPropertyMapping<TResult>(
            Dictionary<string, int> columnMeta,
            TranslateOptions options)
        {
            return PropertyMapCache.GetOrAdd(typeof(TResult), type =>
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && !IsIgnoredProperty(p));

                var propertyMap = new Dictionary<PropertyInfo, int>();

                foreach (var prop in properties)
                {
                    var columnName = FindMatchingColumn(prop.Name, columnMeta, options);
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        propertyMap[prop] = columnMeta[columnName];
                    }
                }

                return propertyMap;
            });
        }

        private static string FindMatchingColumn(
            string propertyName,
            Dictionary<string, int> columnMeta,
            TranslateOptions options)
        {
            // Búsqueda exacta primero
            if (columnMeta.ContainsKey(propertyName))
                return propertyName;

            // Búsqueda ignorando case
            var exactMatch = columnMeta.Keys
                .FirstOrDefault(col => string.Equals(col, propertyName, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
                return exactMatch;

            // Aplicar remoción de prefijos
            var propertyNameWithoutPrefix = RemovePrefixes(propertyName, options.ColumnPrefixesToRemove);
            if (propertyNameWithoutPrefix != propertyName && columnMeta.ContainsKey(propertyNameWithoutPrefix))
                return propertyNameWithoutPrefix;

            // Búsqueda flexible (snake_case to PascalCase, etc.)
            var flexibleMatch = columnMeta.Keys
                .FirstOrDefault(col => IsFlexibleMatch(col, propertyName));
            if (flexibleMatch != null)
                return flexibleMatch;

            return null;
        }

        private static string RemovePrefixes(string input, string[] prefixes)
        {
            if (prefixes == null || !prefixes.Any())
                return input;

            foreach (var prefix in prefixes)
            {
                if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return input.Substring(prefix.Length);
                }
            }
            return input;
        }

        private static bool IsFlexibleMatch(string columnName, string propertyName)
        {
            // Implementar lógica flexible según necesidades
            // Ejemplo: "user_name" -> "UserName"
            var normalizedColumn = columnName.Replace("_", "").Replace(" ", "");
            var normalizedProperty = propertyName.Replace("_", "").Replace(" ", "");

            return string.Equals(normalizedColumn, normalizedProperty, StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return GetDefaultValue(targetType);

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlyingType.IsEnum)
                    return Enum.ToObject(underlyingType, value);

                // Manejo especial para tipos comunes
                if (underlyingType == typeof(Guid))
                    return Guid.Parse(value.ToString());

                if (underlyingType == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(value.ToString());

                return Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"No se puede convertir el valor '{value}' ({value.GetType().Name}) a {underlyingType.Name}", ex);
            }
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool IsNullableType(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static bool IsIgnoredProperty(PropertyInfo property)
        {
            // Podemos usar atributos personalizados para ignorar propiedades
            return property.GetCustomAttribute<IgnoreMappingAttribute>() != null;
        }

        private static bool IsOptionalProperty(PropertyInfo property)
        {
            // Podemos usar atributos personalizados para marcar propiedades como opcionales
            return property.GetCustomAttribute<OptionalMappingAttribute>() != null;
        }
    }

    // Atributos personalizados para controlar el mapeo
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMappingAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalMappingAttribute : Attribute { }
}