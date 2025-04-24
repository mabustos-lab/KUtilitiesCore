using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableUInt64Converter : NullableInnerConverter<ulong>
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