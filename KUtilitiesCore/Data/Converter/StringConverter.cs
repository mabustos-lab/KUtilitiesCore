namespace KUtilitiesCore.Data.Converter
{
    internal class StringConverter : BaseConverter<string>
    {
        #region Methods

        public override bool TryConvert(string value, out string result)
        {
            result = value;

            return true;
        }

        #endregion Methods
    }
}