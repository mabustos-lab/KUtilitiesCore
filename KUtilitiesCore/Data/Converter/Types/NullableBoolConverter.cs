using KUtilitiesCore.Data.Converter.Abstracts;
using System;

namespace KUtilitiesCore.Data.Converter.Types
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