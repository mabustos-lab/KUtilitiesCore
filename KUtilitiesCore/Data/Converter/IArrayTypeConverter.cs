namespace KUtilitiesCore.Data.Converter
{
    public interface IArrayTypeConverter<TTargetType> : ITypeConverter
    {
        #region Methods

        char Separator { get; set; }

        bool TryConvert(string[] value, out TTargetType result);

        #endregion Methods
    }
}