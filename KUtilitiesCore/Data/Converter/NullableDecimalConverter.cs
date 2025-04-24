using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableDecimalConverter : NullableInnerConverter<decimal>
    {
        #region Constructors

        public NullableDecimalConverter()
            : base(new DecimalConverter())
        {
        }

        public NullableDecimalConverter(IFormatProvider formatProvider)
            : base(new DecimalConverter(formatProvider))
        {
        }

        public NullableDecimalConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new DecimalConverter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}