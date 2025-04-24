using System;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableBoolConverter : NullableInnerConverter<bool>
    {
        #region Constructors

        public NullableBoolConverter()
            : base(new BoolConverter())
        {
        }

        public NullableBoolConverter(string trueValue, string falseValue, StringComparison stringComparism)
            : base(new BoolConverter(trueValue, falseValue, stringComparism))
        {
        }

        #endregion Constructors
    }
}