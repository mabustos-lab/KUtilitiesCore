namespace KUtilitiesCore.Data.Converter
{
    public interface ITypeConverterGenericObject : ITypeConverter
    {
        /// <summary>
        /// Intenta convertir el valor de texto especificado a un objeto.
        /// </summary>
        new object TryConvert(string value);
    }
}