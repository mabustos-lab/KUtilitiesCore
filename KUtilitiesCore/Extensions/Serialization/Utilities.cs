using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;




#if NET6_0_OR_GREATER // Incluye .NET 6, 7, 8 y superiores
using System.Text.Json;
using System.Text.Json.Serialization;
#else // Para .NET Framework 4.x y .NET Core/.NET 5 anteriores
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

namespace KUtilitiesCore.Extensions.Serialization
{
    public static class Utilities
    {
#if NET6_0_OR_GREATER
        // --- Opciones de Serialización JSON para System.Text.Json ---
        private static readonly JsonSerializerOptions _jsonOptionsDefault = new()
        {
            PropertyNameCaseInsensitive = true, // Ignora mayúsculas/minúsculas en propiedades
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Usa camelCase para nombres de propiedad (ej. miPropiedad)
            WriteIndented = true, // Escribe el JSON con formato indentado para legibilidad
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // No incluye propiedades con valor null
            // podemos agregar mas opciones
        };

        /// <summary>
        /// Serializa un objeto a una cadena JSON usando System.Text.Json.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a serializar.</typeparam>
        /// <param name="source">El objeto a serializar.</param>
        /// <param name="options">Opciones personalizadas de serialización JSON. Si es null, se usan las opciones por defecto.</param>
        /// <returns>Una cadena que representa el objeto en formato JSON.</returns>
        /// <exception cref="ArgumentNullException">Si el objeto es null.</exception>
        /// <exception cref="JsonException">Si ocurre un error durante la serialización.</exception>
        public static string ToJson<T>(this T source, JsonSerializerOptions? options = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "El objeto a serializar no puede ser null.");
            }

            try
            {
                return JsonSerializer.Serialize(source, options ?? _jsonOptionsDefault);
            }
            catch (Exception ex) 
            {
                throw new JsonException($"Error al serializar el objeto de tipo {typeof(T).FullName} a JSON.", ex);
            }
        }

        /// <summary>
        /// Deserializa una cadena JSON a un objeto usando System.Text.Json.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto al que deserializar.</typeparam>
        /// <param name="json">La cadena JSON a deserializar.</param>
        /// <param name="options">Opciones personalizadas de deserialización JSON. Si es null, se usan las opciones por defecto.</param>
        /// <returns>Una instancia del objeto de tipo T.</returns>
        /// <exception cref="ArgumentException">Si la cadena JSON es null o vacía.</exception>
        /// <exception cref="JsonException">Si ocurre un error durante la deserialización.</exception>
        public static T FromJson<T>(this string json, JsonSerializerOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("La cadena JSON no puede ser null o vacía.", nameof(json));
            }
            try
            {
                var result = JsonSerializer.Deserialize<T>(json, options ?? _jsonOptionsDefault);
                return result is null
                    ? throw new JsonException($"La deserialización retornó null para el tipo {typeof(T).FullName}. Se esperaba un objeto válido.")
                    : result;
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error al deserializar JSON al tipo {typeof(T).FullName}.", ex);
            }
        }

#else
        // --- Opciones de Serialización JSON para Newtonsoft.Json ---

        private static readonly JsonSerializerSettings _jsonSettingsDefault = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(), // Usa camelCase
            Formatting = Formatting.Indented, // Formato indentado
            NullValueHandling = NullValueHandling.Ignore, // Ignora valores null
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore // Ignora referencias circulares
            // podemos agregar mas opciones
        };

        /// <summary>
        /// Serializa un objeto a una cadena JSON usando Newtonsoft.Json.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a serializar.</typeparam>
        /// <param name="source">El objeto a serializar.</param>
        /// <param name="settings">Configuración personalizada de serialización JSON. Si es null, se usa la configuración por defecto.</param>
        /// <returns>Una cadena que representa el objeto en formato JSON.</returns>
        /// <exception cref="ArgumentNullException">Si el objeto es null.</exception>
        /// <exception cref="JsonSerializationException">Si ocurre un error durante la serialización.</exception>
        public static string ToJson<T>(this T source, JsonSerializerSettings? settings = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "El objeto a serializar no puede ser null.");
            }

            try
            {
                return JsonConvert.SerializeObject(source, settings ?? _jsonSettingsDefault);
            }
            catch (JsonSerializationException jsex) // Captura la excepción específica de Newtonsoft
            {
                // Considera loggear el error aquí
                throw new JsonSerializationException($"Error al serializar el objeto de tipo {typeof(T).FullName} a JSON.", jsex);
            }
            catch (Exception ex) // Captura cualquier otra excepción inesperada
            {
                // Considera loggear el error aquí
                throw new JsonSerializationException($"Error inesperado al serializar el objeto de tipo {typeof(T).FullName} a JSON.", ex);
            }
        }

         /// <summary>
        /// Deserializa una cadena JSON a un objeto usando Newtonsoft.Json.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto al que deserializar.</typeparam>
        /// <param name="json">La cadena JSON a deserializar.</param>
        /// <param name="settings">Configuración personalizada de deserialización JSON. Si es null, se usa la configuración por defecto.</param>
        /// <returns>Una instancia del objeto de tipo T.</returns>
        /// <exception cref="ArgumentException">Si la cadena JSON es null o vacía.</exception>
        /// <exception cref="JsonSerializationException">Si ocurre un error durante la deserialización.</exception>
        public static T FromJson<T>(this string json, JsonSerializerSettings? settings = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("La cadena JSON no puede ser null o vacía.", nameof(json));
            }
            try
            {
                var result = JsonConvert.DeserializeObject<T>(json, settings ?? _jsonSettingsDefault);
                return result is null
                    ? throw new JsonSerializationException($"La deserialización retornó null para el tipo {typeof(T).FullName}. Se esperaba un objeto válido.")
                    : result;
            }
            catch (Exception ex)
            {
                // Considera loggear el error aquí
                throw new JsonSerializationException($"Error al deserializar JSON al tipo {typeof(T).FullName}.", ex);
            }
        }
#endif

        // --- Serialización XML ---

        /// <summary>
        /// Serializa un objeto a una cadena XML usando System.Xml.Serialization.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto a serializar. Debe ser serializable en XML.</typeparam>
        /// <param name="obj">El objeto a serializar.</param>
        /// <param name="omitXmlDeclaration">Indica si se debe omitir la declaración XML (<?xml version="1.0"...?>).</param>
        /// <param name="namespaces">Espacios de nombres XML a utilizar durante la serialización.</param>
        /// <returns>Una cadena que representa el objeto en formato XML.</returns>
        /// <exception cref="ArgumentNullException">Si el objeto es null.</exception>
        /// <exception cref="InvalidOperationException">Si ocurre un error durante la serialización XML (p.ej., el tipo no es serializable).</exception>
        public static string ToXml<T>(this T obj, bool omitXmlDeclaration = false, XmlSerializerNamespaces? namespaces = null)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj), "El objeto a serializar no puede ser null.");
            }

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                using var stringWriter = new StringWriterUtf8(); // Usar StringWriter con codificación UTF-8
                var writerSettings = new System.Xml.XmlWriterSettings
                {
                    Indent = true, // Indentar el XML para legibilidad
                    OmitXmlDeclaration = omitXmlDeclaration,
                    Encoding = Encoding.UTF8 // Asegurar UTF-8
                };

                using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, writerSettings))
                {
                    // Usar los namespaces proporcionados o crear uno vacío por defecto para evitar xsi/xsd
                    XmlSerializerNamespaces ns = namespaces ?? new XmlSerializerNamespaces([System.Xml.XmlQualifiedName.Empty]);
                    xmlSerializer.Serialize(xmlWriter, obj, ns);
                }
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                // Considera loggear el error aquí
                throw new InvalidOperationException($"Error al serializar el objeto de tipo {typeof(T).FullName} a XML.", ex);
            }
        }

        /// <summary>
        /// Deserializa una cadena XML a un objeto usando System.Xml.Serialization.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto al que deserializar.</typeparam>
        /// <param name="xml">La cadena XML a deserializar.</param>
        /// <returns>Una instancia del objeto de tipo T.</returns>
        /// <exception cref="ArgumentException">Si la cadena XML es null o vacía.</exception>
        /// <exception cref="InvalidOperationException">Si ocurre un error durante la deserialización XML.</exception>
        public static T FromXml<T>(this string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentException("La cadena XML no puede ser null o vacía.", nameof(xml));
            }

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                using var stringReader = new StringReader(xml);
                T? result= (T?)xmlSerializer.Deserialize(stringReader);
                return result is null
                    ? throw new InvalidOperationException($"La deserialización retornó null para el tipo {typeof(T).FullName}. Se esperaba un objeto válido.")
                    : result;
            }
            catch (Exception ex)
            {
                // Considera loggear el error aquí
                throw new InvalidOperationException($"Error al deserializar XML al tipo {typeof(T).FullName}.", ex);
            }
        }

        // Clase auxiliar para asegurar codificación UTF-8 en StringWriter
        private sealed class StringWriterUtf8 : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
