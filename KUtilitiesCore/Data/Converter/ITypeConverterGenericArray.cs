namespace KUtilitiesCore.Data.Converter
{
    /// <summary>
    /// Interfaz para convertir un arreglo de cadenas a un arreglo de objetos de tipo específico.
    /// </summary>
    public interface ITypeConverterGenericArray : ITypeConverter
    {
        /// <summary>
        /// Intenta convertir un arreglo de cadenas a un arreglo de objetos.
        /// </summary>
        /// <param name="value">Arreglo de cadenas a convertir.</param>
        /// <returns>Arreglo de objetos convertidos; puede estar vacío si la conversión falla.</returns>
        object[] TryConvert(string[] value);
    }
}