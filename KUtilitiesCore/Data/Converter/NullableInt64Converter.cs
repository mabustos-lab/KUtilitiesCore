using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableInt64Converter : NullableInnerConverter<Int64>
    {
        #region Constructors

        public NullableInt64Converter()
            : base(new Int64Converter())
        {
        }

        public NullableInt64Converter(IFormatProvider formatProvider)
            : base(new Int64Converter(formatProvider))
        {
        }

        public NullableInt64Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new Int64Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}