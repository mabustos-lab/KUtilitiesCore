using KUtilitiesCore.Data.Converter;
using System;

namespace KUtilities.Data.Converter
{
    /// <summary>
    /// Delegado que define un método para obtener un convertidor de tipo basado en el tipo de campo.
    /// </summary>
    /// <param name="fieldType">El tipo del campo para el cual se requiere un convertidor.</param>
    /// <returns>Una instancia de <see cref="ITypeConverter"/> que puede manejar el tipo especificado.</returns>
    public delegate ITypeConverter ConverterByType(Type fieldType);
    /// <summary>
    /// Proveedor de convertidores de tipo que permite registrar y resolver convertidores personalizados.
    /// </summary>
    public interface ITypeConverterProvider
    {
        #region Methods

        /// <summary>
        /// Agrega un convertidor de tipo específico al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">El tipo de destino que el convertidor manejará.</typeparam>
        /// <param name="typeConverter">El convertidor de tipo a registrar.</param>
        /// <returns>La instancia actual de <see cref="ITypeConverterProvider"/> para encadenar llamadas.</returns>
        ITypeConverterProvider Add<TTargetType>(ITypeConverter<TTargetType> typeConverter);

        /// <summary>
        /// Agrega un convertidor de tipo para colecciones al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">El tipo de destino que el convertidor manejará.</typeparam>
        /// <param name="typeConverter">El convertidor de tipo para colecciones a registrar.</param>
        /// <returns>La instancia actual de <see cref="ITypeConverterProvider"/> para encadenar llamadas.</returns>
        ITypeConverterProvider Add<TTargetType>(IArrayTypeConverter<TTargetType> typeConverter);

        /// <summary>
        /// Agrega un convertidor para enumeraciones al proveedor.
        /// </summary>
        /// <typeparam name="TTargetType">El tipo de enumeración que el convertidor manejará.</typeparam>
        /// <returns>La instancia actual de <see cref="ITypeConverterProvider"/> para encadenar llamadas.</returns>
        ITypeConverterProvider AddEnum<TTargetType>() where TTargetType : struct, IConvertible;

        /// <summary>
        /// Verifica si existe un convertidor registrado para un tipo de destino específico.
        /// </summary>
        /// <param name="targetType">El tipo de destino a verificar.</param>
        /// <returns>True si existe un convertidor para el tipo especificado; de lo contrario, false.</returns>
        bool ContainsConverterType(Type targetType);

        /// <summary>
        /// Obtiene o establece un delegado para resolver convertidores personalizados basados en el tipo.
        /// </summary>
        ConverterByType GetCustomConverter { get; set; }

        /// <summary>
        /// Resuelve un convertidor para un tipo de destino específico.
        /// </summary>
        /// <param name="targetType">El tipo de destino para el cual se requiere un convertidor.</param>
        /// <returns>Una instancia de <see cref="ITypeConverter"/> que puede manejar el tipo especificado.</returns>
        ITypeConverter Resolve(Type targetType);

        /// <summary>
        /// Resuelve un convertidor para un tipo de destino genérico.
        /// </summary>
        /// <typeparam name="TTargetType">El tipo de destino que el convertidor manejará.</typeparam>
        /// <returns>Una instancia de <see cref="ITypeConverter{TTargetType}"/> que puede manejar el tipo especificado.</returns>
        ITypeConverter<TTargetType> Resolve<TTargetType>();

        /// <summary>
        /// Resuelve un convertidor para colecciones de un tipo de destino genérico.
        /// </summary>
        /// <typeparam name="TTargetType">El tipo de destino que el convertidor manejará.</typeparam>
        /// <returns>Una instancia de <see cref="IArrayTypeConverter{TTargetType}"/> que puede manejar el tipo especificado.</returns>
        IArrayTypeConverter<TTargetType> ResolveCollection<TTargetType>();

        #endregion Methods
    }
}