using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableInt16Converter : NullableInnerConverter<short>
    {
        #region Constructors

        public NullableInt16Converter()
            : base(new Int16Converter())
        {
        }

        public NullableInt16Converter(IFormatProvider formatProvider)
            : base(new Int16Converter(formatProvider))
        {
        }

        public NullableInt16Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new Int16Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}