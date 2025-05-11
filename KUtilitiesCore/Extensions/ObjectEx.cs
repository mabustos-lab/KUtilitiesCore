using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#if NETFRAMEWORK
using Newtonsoft.Json;
#endif
#if !NETFRAMEWORK
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace KUtilitiesCore.Extensions
{
    public static class ObjectEx
    {
        // Caché estático y seguro para hilos para almacenar los mapeos de propiedades ya calculados.
        // La clave es una tupla de los tipos (Origen, Destino).
        // El valor es una lista de tuplas, cada una conteniendo el PropertyInfo del origen y del destino.
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, Dictionary<PropertyInfo, PropertyInfo>> _propertyCache =
            new ConcurrentDictionary<Tuple<Type, Type>, Dictionary<PropertyInfo, PropertyInfo>>();

        /// <summary>
        /// Mapea las propiedades de una fuente a un destino de tipos compatibles.
        /// </summary>
        /// <typeparam name="TSource">Tipo de la fuente.</typeparam>
        /// <typeparam name="TDestination">Tipo del destino.</typeparam>
        /// <param name="source">La instancia fuente a mapear.</param>
        /// <param name="destination">La instancia destino que recibirá los valores.</param>
        /// <returns>La instancia destino con los valores mapeados.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="source"/> o <paramref name="destination"/> son nulos.</exception>
        public static TDestination MapPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var typePair = GetTypesPair(source, destination);

            var propertyMapping = GetPropertyMapping(typePair, source, destination);

            CopyPropertyValues(source, destination, propertyMapping);

            return destination;
        }

        /// <summary>
        /// Obtiene un par de tipos representado como una tupla, a partir de la fuente y el destino.
        /// </summary>
        /// <typeparam name="TSource">Tipo de la fuente.</typeparam>
        /// <typeparam name="TDestination">Tipo del destino.</typeparam>
        /// <param name="source">La instancia fuente.</param>
        /// <param name="destination">La instancia destino.</param>
        /// <returns>Un par de <see cref="Type"/> representado como una tupla.</returns>
        private static Tuple<Type, Type> GetTypesPair<TSource, TDestination>(TSource source, TDestination destination)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            return Tuple.Create(sourceType, destinationType);
        }

        /// <summary>
        /// Obtiene el mapeo de propiedades entre dos tipos.
        /// Utiliza un diccionario concurrente para cachear los mapeos ya computados.
        /// </summary>
        /// <param name="typePair">El par de tipos para los cuales se obtiene el mapeo.</param>
        /// <param name="source">La instancia fuente.</param>
        /// <param name="destination">La instancia destino.</param>
        /// <returns>Un diccionario que mapea propiedades de origen a destino.</returns>
        private static Dictionary<PropertyInfo, PropertyInfo> GetPropertyMapping(
            Tuple<Type, Type> typePair,
            object source,
            object destination)
        {
            return _propertyCache.GetOrAdd(typePair, ComputePropertyMapping);

            Dictionary<PropertyInfo, PropertyInfo> ComputePropertyMapping(Tuple<Type, Type> types)
            {
                var sourceProperties = types.Item1.GetProperties();
                var destinationProperties = types.Item2.GetProperties();

                return GetMatchingProperties(sourceProperties, destinationProperties);
            }
        }

        /// <summary>
        /// Obtiene las propiedades que coinciden entre la fuente y el destino.
        /// </summary>
        /// <param name="sourceProperties">Arreglo de <see cref="PropertyInfo"/> de la fuente.</param>
        /// <param name="destinationProperties">Arreglo de <see cref="PropertyInfo"/> del destino.</param>
        /// <returns>Un diccionario que mapea propiedades de origen a destino.</returns>
        private static Dictionary<PropertyInfo, PropertyInfo> GetMatchingProperties(PropertyInfo[] sourceProperties, PropertyInfo[] destinationProperties)
        {
            return destinationProperties
                .Select(destProp => FindMatchingProperty(destProp, sourceProperties))
                .Where(match => match != null)
                .Cast<Tuple<PropertyInfo, PropertyInfo>>()
                .ToDictionary(match => match.Item1, match => match.Item2);
        }

        /// <summary>
        /// Busca una propiedad en el origen que coincida con una propiedad del destino.
        /// </summary>
        /// <param name="destinationProperty">La propiedad del destino a buscar.</param>
        /// <param name="sourceProperties">Arreglo de <see cref="PropertyInfo"/> de la fuente.</param>
        /// <returns>
        /// Un <see cref="Tuple{T1, T2}"/> que contiene la propiedad de origen y destino si existe una coincidencia;
        /// null en caso contrario.
        /// </returns>
        private static Tuple<PropertyInfo, PropertyInfo> FindMatchingProperty(PropertyInfo destinationProperty, PropertyInfo[] sourceProperties)
        {
            var sourceProp = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);

            if (sourceProp?.CanRead == true &&
                destinationProperty.CanWrite &&
                destinationProperty.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                return Tuple.Create(sourceProp, destinationProperty);
            }

            return null;
        }

        /// <summary>
        /// Copia los valores de las propiedades de la fuente al destino utilizando el mapeo proporcionado.
        /// </summary>
        /// <param name="source">La instancia fuente.</param>
        /// <param name="destination">La instancia destino.</param>
        /// <param name="propertyMapping">El diccionario que mapea propiedades de origen a destino.</param>
        private static void CopyPropertyValues(object source, object destination, Dictionary<PropertyInfo, PropertyInfo> propertyMapping)
        {
            foreach (var item in propertyMapping)
            {
                var sourceProp = item.Key;
                var destProp = item.Value;
                try
                {
                    var value = sourceProp.GetValue(source);
                    destProp.SetValue(destination, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al mapear propiedad '{sourceProp.Name}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Realiza una copia profunda de un objeto usando serialización JSON.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a copiar.</typeparam>
        /// <param name="obj">Objeto origen para la copia.</param>
        /// <returns>Nueva instancia que es una copia profunda del objeto original.</returns>
        /// <exception cref="ArgumentNullException">Se produce si el objeto es nulo.</exception>
        public static T CreateDeepCopy<T>(this T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "El objeto no puede ser nulo.");
            }

#if NETFRAMEWORK
            // Para .NET Framework usamos Newtonsoft.Json
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(obj, settings);
            return JsonConvert.DeserializeObject<T>(json);
#else
        // Para .NET 8 (y demás versiones .NET Core) usamos System.Text.Json
        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        string json = JsonSerializer.Serialize(obj, options);
        return JsonSerializer.Deserialize<T>(json, options)!;
#endif
        }

    }
}