using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace KUtilitiesCore.Data
{
    /// <summary>
    /// Clase interna de utilidad para registrar clases de metadatos asociadas mediante <see cref="MetadataTypeAttribute"/>.
    /// </summary>
    internal static class MetadataHelper
    {
        private static readonly ConcurrentDictionary<Type, Type?> MetaDataDictionary = new ConcurrentDictionary<Type, Type?>();

        /// <summary>
        /// Registra la clase de metadatos para el tipo de modelo especificado si tiene el atributo <see cref="MetadataTypeAttribute"/>.
        /// </summary>
        /// <param name="modelType">El tipo de la clase que se desea validar.</param>
        public static void RegisterMetadataClass(Type modelType)
        {
            if (modelType == null) return;

            // Intentamos obtener o agregar al diccionario para evitar registros duplicados costosos
            MetaDataDictionary.GetOrAdd(modelType, type =>
            {
                var metadataType = GetMetadataType(type);
                if (metadataType != null)
                {
                    // Registra el proveedor de descripción de tipos asociado a la clase de metadatos
                    TypeDescriptor.AddProviderTransparent(
                        new AssociatedMetadataTypeTypeDescriptionProvider(type, metadataType), 
                        type);
                }
                return metadataType;
            });
        }

        private static Type? GetMetadataType(Type type)
        {
            var attribute = (MetadataTypeAttribute?)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
            return attribute?.MetadataClassType;
        }
    }
}
