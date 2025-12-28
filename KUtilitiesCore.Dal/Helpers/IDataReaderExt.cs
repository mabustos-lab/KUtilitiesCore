using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.Dal.Helpers
{
    public static class IDataReaderExt
    {
        // Cache para funciones de mapeo compiladas
        private static readonly ConcurrentDictionary<string, Func<IDataReader, object>> _translatorCache =
            new ConcurrentDictionary<string, Func<IDataReader, object>>();

        // Cache para metadatos de propiedades
        private static readonly ConcurrentDictionary<Type, PropertyMapper[]> _propertyMapperCache =
            new ConcurrentDictionary<Type, PropertyMapper[]>();

        // Cache para metadatos de columnas
        private static readonly ConcurrentDictionary<int, Dictionary<string, int>> _columnMetaCache =
            new ConcurrentDictionary<int, Dictionary<string, int>>();

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

            // Obtener metadatos de columnas
            var columnMeta = GetColumnMetaData(reader);

            // Obtener o crear función de mapeo compilada
            var translator = GetOrCreateTranslator<TResult>(columnMeta, options);

            // Validar mapeo estricto si es requerido
            if (options.StrictMapping)
            {
                ValidateStrictMapping<TResult>(columnMeta, options);
            }

            // Procesar filas
            while (reader.Read())
            {
                yield return (TResult)translator(reader);
            }
        }

        private static Dictionary<string, int> GetColumnMetaData(IDataReader reader)
        {
            var readerHash = reader.GetHashCode();

            return _columnMetaCache.GetOrAdd(readerHash, _ =>
            {
                var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columnMap[reader.GetName(i)] = i;
                }
                return columnMap;
            });
        }

        private static Func<IDataReader, object> GetOrCreateTranslator<TResult>(
            Dictionary<string, int> columnMeta,
            TranslateOptions options) where TResult : new()
        {
            string cacheKey = $"{typeof(TResult).FullName}_{options.GetHashCode()}_{columnMeta.Count}";

            return _translatorCache.GetOrAdd(cacheKey, _ =>
            {
                // Obtener mapeadores de propiedades
                var propertyMappers = GetPropertyMappers<TResult>(columnMeta, options);

                // Compilar función de mapeo
                return CompileTranslator<TResult>(propertyMappers);
            });
        }

        private static PropertyMapper[] GetPropertyMappers<TResult>(
            Dictionary<string, int> columnMeta,
            TranslateOptions options) where TResult : new()
        {
            return _propertyMapperCache.GetOrAdd(typeof(TResult), type =>
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite && !IsIgnoredProperty(p))
                    .ToList();

                var mappers = new List<PropertyMapper>(properties.Count);

                foreach (var property in properties)
                {
                    var columnIndex = FindMatchingColumnIndex(property.Name, columnMeta, options);
                    if (columnIndex >= 0)
                    {
                        mappers.Add(new PropertyMapper
                        {
                            Property = property,
                            ColumnIndex = columnIndex,
                            Setter = CreatePropertySetter(property),
                            Converter = CreateValueConverter(property.PropertyType)
                        });
                    }
                }

                return mappers.ToArray();
            });
        }

        private static Func<IDataReader, object> CompileTranslator<TResult>(PropertyMapper[] propertyMappers) where TResult : new()
        {
            // Crear expresión lambda para mapeo optimizado
            var readerParam = Expression.Parameter(typeof(IDataReader), "reader");
            var instanceVar = Expression.Variable(typeof(TResult), "instance");

            var expressions = new List<Expression>
            {
                Expression.Assign(instanceVar, Expression.New(typeof(TResult)))
            };

            // Agregar asignaciones para cada propiedad
            foreach (var mapper in propertyMappers)
            {
                try
                {
                    // Obtener valor del reader
                    var getValueCall = Expression.Call(
                        readerParam,
                        nameof(IDataReader.GetValue),
                        null,
                        Expression.Constant(mapper.ColumnIndex));

                    // Convertir valor
                    var convertedValue = Expression.Invoke(
                        Expression.Constant(mapper.Converter),
                        getValueCall);

                    // Asignar a propiedad
                    var propertyExpr = Expression.Property(instanceVar, mapper.Property);
                    var assignExpr = Expression.Assign(propertyExpr, Expression.Convert(convertedValue, mapper.Property.PropertyType));

                    expressions.Add(assignExpr);
                }
                catch (Exception ex)
                {
                    // Fallback a mapeo por reflexión si la compilación falla
                    System.Diagnostics.Debug.WriteLine($"Error compilando mapeo para {mapper.Property.Name}: {ex.Message}");
                }
            }

            expressions.Add(instanceVar);

            var block = Expression.Block(new[] { instanceVar }, expressions);
            var lambda = Expression.Lambda<Func<IDataReader, object>>(block, readerParam);

            return lambda.Compile();
        }

        private static Action<object, object> CreatePropertySetter(PropertyInfo property)
        {
            // Crear setter compilado usando Expression Trees
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            var instanceCast = Expression.Convert(instanceParam, property.DeclaringType);
            var valueCast = Expression.Convert(valueParam, property.PropertyType);
            var propertyAccess = Expression.Property(instanceCast, property);
            var assign = Expression.Assign(propertyAccess, valueCast);

            var lambda = Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam);
            return lambda.Compile();
        }

        private static Func<object, object> CreateValueConverter(Type targetType)
        {
            return value =>
            {
                if (value == null || value == DBNull.Value)
                    return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                        ? Activator.CreateInstance(targetType)
                        : null;

                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                try
                {
                    if (underlyingType.IsEnum)
                        return Enum.ToObject(underlyingType, value);

                    if (underlyingType == typeof(Guid))
                        return Guid.Parse(value.ToString());

                    if (underlyingType == typeof(DateTimeOffset))
                        return DateTimeOffset.Parse(value.ToString());

                    return Convert.ChangeType(value, underlyingType);
                }
                catch
                {
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
            };
        }

        private static int FindMatchingColumnIndex(string propertyName, Dictionary<string, int> columnMeta, TranslateOptions options)
        {
            // Búsqueda exacta
            if (columnMeta.TryGetValue(propertyName, out int index))
                return index;

            // Búsqueda case-insensitive
            var exactMatch = columnMeta.FirstOrDefault(kv =>
                string.Equals(kv.Key, propertyName, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(exactMatch.Key))
                return exactMatch.Value;

            // Remover prefijos
            var propertyNameWithoutPrefix = RemovePrefixes(propertyName, options.ColumnPrefixesToRemove);
            if (propertyNameWithoutPrefix != propertyName && columnMeta.TryGetValue(propertyNameWithoutPrefix, out index))
                return index;

            // Búsqueda flexible
            var normalizedProperty = NormalizeName(propertyName);
            foreach (var column in columnMeta.Keys)
            {
                if (string.Equals(NormalizeName(column), normalizedProperty, StringComparison.OrdinalIgnoreCase))
                    return columnMeta[column];
            }

            return -1;
        }

        private static string NormalizeName(string name)
        {
            return name?.Replace("_", "").Replace(" ", "").Trim() ?? string.Empty;
        }

        private static string RemovePrefixes(string input, string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                return input;

            foreach (var prefix in prefixes)
            {
                if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return input.Substring(prefix.Length);
            }
            return input;
        }

        private static void ValidateStrictMapping<TResult>(Dictionary<string, int> columnMeta, TranslateOptions options)
        {
            var missingProperties = new List<string>();
            var properties = typeof(TResult).GetProperties()
                .Where(p => p.CanWrite && !IsIgnoredProperty(p) && !IsOptionalProperty(p))
                .ToArray();

            foreach (var prop in properties)
            {
                if (FindMatchingColumnIndex(prop.Name, columnMeta, options) < 0)
                    missingProperties.Add(prop.Name);
            }

            if (missingProperties.Any())
            {
                throw new InvalidOperationException(
                    $"Mapeo estricto fallido para {typeof(TResult).Name}. " +
                    $"Propiedades no mapeadas: {string.Join(", ", missingProperties)}");
            }
        }

        internal static string[] GetPropertiesRequired<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && !IsIgnoredProperty(p))
                .Select(p => p.Name)
                .ToArray();
        }

        private static bool IsIgnoredProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute<IgnoreMappingAttribute>() != null;
        }

        private static bool IsOptionalProperty(PropertyInfo property)
        {
            return property.GetCustomAttribute<OptionalMappingAttribute>() != null;
        }

        private class PropertyMapper
        {
            public PropertyInfo Property { get; set; }
            public int ColumnIndex { get; set; }
            public Action<object, object> Setter { get; set; }
            public Func<object, object> Converter { get; set; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMappingAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalMappingAttribute : Attribute { }
}