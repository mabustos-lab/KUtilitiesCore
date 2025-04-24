using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableUInt32Converter : NullableInnerConverter<uint>
    {
        #region Constructors

        public NullableUInt32Converter()
            : base(new UInt32Converter())
        {
        }

        public NullableUInt32Converter(IFormatProvider formatProvider)
            : base(new UInt32Converter(formatProvider))
        {
        }

        public NullableUInt32Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new UInt32Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}