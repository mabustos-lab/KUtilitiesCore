using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableUInt64Converter : NullableInnerConverter<UInt64>
    {
        #region Constructors

        public NullableUInt64Converter()
            : base(new UInt64Converter())

        {
        }

        public NullableUInt64Converter(IFormatProvider formatProvider)
            : base(new UInt64Converter(formatProvider))
        {
        }

        public NullableUInt64Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new UInt64Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}