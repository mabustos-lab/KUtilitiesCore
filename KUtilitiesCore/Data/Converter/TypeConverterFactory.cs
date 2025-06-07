using System;

namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Fábrica para obtener el proveedor de convertidores de tipo.
    /// Implementa el patrón Singleton para asegurar una única instancia global.
    /// </summary>
    public class TypeConverterFactory
    {
        /// <summary>
        /// Instancia singleton de la fábrica, inicializada de manera diferida (lazy).
        /// </summary>
        private static readonly Lazy<TypeConverterFactory> instance = new Lazy<TypeConverterFactory>(() => new TypeConverterFactory());

        private readonly ITypeConverterProvider provider;

        /// <summary>
        /// Constructor privado para evitar la creación de instancias externas.
        /// </summary>
        private TypeConverterFactory()
        {
            provider = new TypeConverterProvider();
        }

        /// <summary>
        /// Proveedor global de convertidores de tipo.
        /// </summary>
        public static ITypeConverterProvider Provider => instance.Value.provider;
    }
}