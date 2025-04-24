using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableUInt16Converter : NullableInnerConverter<ushort>
    {
        #region Constructors

        public NullableUInt16Converter()
            : base(new UInt16Converter())
        {
        }

        public NullableUInt16Converter(IFormatProvider formatProvider)
            : base(new UInt16Converter(formatProvider))
        {
        }

        public NullableUInt16Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new UInt16Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}