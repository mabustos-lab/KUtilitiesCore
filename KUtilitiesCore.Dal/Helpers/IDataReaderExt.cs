using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Provides extension methods for IDataReader and DbDataReader to translate data records into objects.
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Translates the IDataReader records into an enumerable of objects of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="reader">The IDataReader to read from.</param>
        /// <returns>An enumerable of objects of type T.</returns>
        public static IEnumerable<T> Translate<T>(this IDataReader reader) where T : new()
        {
            return reader.Translate<T>(new TranslateOptions());
        }

        /// <summary>
        /// Translates the IDataReader records into an enumerable of objects of type T, using the specified translation options.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="reader">The IDataReader to read from.</param>
        /// <param name="options">The options to use for translation.</param>
        /// <returns>An enumerable of objects of type T.</returns>
        public static IEnumerable<T> Translate<T>(this IDataReader reader, TranslateOptions options) where T : new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var translator = new Translator<T>(reader, options);
            while (reader.Read())
            {
                yield return translator.Translate(reader);
            }
        }
    }

    /// <summary>
    /// Handles the translation of an IDataReader record to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of object to translate to.</typeparam>
    internal class Translator<T> where T : new()
    {
        private readonly List<Mapping> _mappings;

        /// <summary>
        /// Initializes a new instance of the Translator class.
        /// </summary>
        /// <param name="reader">The IDataReader to get schema information from.</param>
        /// <param name="options">The translation options.</param>
        public Translator(IDataReader reader, TranslateOptions options)
        {
            var columnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames[reader.GetName(i)] = i;
            }

            _mappings = new List<Mapping>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetCustomAttribute<IgnoreMappingAttribute>() == null);

            foreach (var property in properties)
            {
                if (TryFindColumnIndex(property.Name, columnNames, options, out int columnIndex))
                {
                    _mappings.Add(new Mapping(property, columnIndex));
                }
                else if (options.StrictMapping && property.GetCustomAttribute<OptionalMappingAttribute>() == null)
                {
                    throw new InvalidOperationException($"Property '{property.Name}' not found in the data reader.");
                }
            }
        }

        /// <summary>
        /// Translates a single IDataReader record to an object of type T.
        /// </summary>
        /// <param name="record">The IDataRecord to translate.</param>
        /// <returns>An object of type T.</returns>
        public T Translate(IDataRecord record)
        {
            var item = new T();
            foreach (var mapping in _mappings)
            {
                var value = record.GetValue(mapping.ColumnIndex);
                if (value != DBNull.Value && value != null)
                {
                    var convertedValue = ConvertValue(value, mapping.Property.PropertyType);
                    mapping.Property.SetValue(item, convertedValue);
                }
            }
            return item;
        }
        
        private static bool TryFindColumnIndex(string propertyName, Dictionary<string, int> columnNames, TranslateOptions options, out int index)
        {
            // Direct match
            if (columnNames.TryGetValue(propertyName, out index))
            {
                return true;
            }

            // Match without prefixes
            var propertyNameWithoutPrefix = RemovePrefixes(propertyName, options.ColumnPrefixesToRemove);
            if (propertyNameWithoutPrefix != propertyName && columnNames.TryGetValue(propertyNameWithoutPrefix, out index))
            {
                return true;
            }
            
            // Flexible match
            var normalizedProperty = NormalizeName(propertyName);
            foreach (var columnName in columnNames.Keys)
            {
                if (NormalizeName(columnName).Equals(normalizedProperty, StringComparison.OrdinalIgnoreCase))
                {
                    index = columnNames[columnName];
                    return true;
                }
            }
            
            return false;
        }

        private static string RemovePrefixes(string input, string[] prefixes)
        {
            if (prefixes == null) return input;
            foreach (var prefix in prefixes)
            {
                if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return input.Substring(prefix.Length);
                }
            }
            return input;
        }

        private static string NormalizeName(string name)
        {
            return name.Replace("_", "").Replace(" ", "").Trim();
        }

        private object ConvertValue(object value, Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType.IsEnum)
            {
                return Enum.ToObject(underlyingType, value);
            }

            return Convert.ChangeType(value, underlyingType);
        }
    }

    /// <summary>
    /// Represents the mapping between a property and a column index.
    /// </summary>
    internal class Mapping
    {
        public PropertyInfo Property { get; }
        public int ColumnIndex { get; }

        public Mapping(PropertyInfo property, int columnIndex)
        {
            Property = property;
            ColumnIndex = columnIndex;
        }
    }

    /// <summary>
    /// When applied to a property, it will be ignored during the translation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMappingAttribute : Attribute { }

    /// <summary>
    /// When applied to a property, it will be considered optional during strict mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalMappingAttribute : Attribute { }
}