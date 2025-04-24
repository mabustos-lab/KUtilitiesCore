namespace KUtilitiesCore.Data.Converter
{
    public interface ITypeConverterGenericObject : ITypeConverter
    {
        object TryConvert(string value);
    }
}