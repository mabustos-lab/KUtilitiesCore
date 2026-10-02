namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Extiende <see cref="ITypeConverter"/> para convertidores que devuelven el
    /// resultado de la conversión como <see cref="object"/> genérico, permitiendo
    /// reutilizarlos cuando el tipo destino no se conoce en tiempo de compilación.
    /// </summary>
    public interface ITypeConverterGenericObject : ITypeConverter
    {
        /// <summary>
        /// Intenta convertir el valor de texto especificado a un objeto.
        /// </summary>
        new object TryConvert(string value);
    }
}