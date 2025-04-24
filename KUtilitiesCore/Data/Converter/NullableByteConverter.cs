using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableByteConverter : NullableInnerConverter<byte>
    {
        #region Constructors

        public NullableByteConverter()
            : base(new ByteConverter())
        {
        }

        public NullableByteConverter(IFormatProvider formatProvider)
            : base(new ByteConverter(formatProvider))
        {
        }

        public NullableByteConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new ByteConverter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}