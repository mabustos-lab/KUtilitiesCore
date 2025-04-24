namespace KUtilitiesCore.Data.Converter
{
    public interface ITypeConverterGenericArray : ITypeConverter
    {
        #region Methods

        object[] TryConvert(string[] value);

        #endregion Methods
    }
}