using KUtilitiesCore.Data.Converter.Abstracts;

namespace KUtilitiesCore.Data.Converter.Types
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