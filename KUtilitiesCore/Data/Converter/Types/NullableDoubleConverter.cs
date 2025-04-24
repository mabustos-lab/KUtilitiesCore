using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableDoubleConverter : NullableInnerConverter<double>
    {
        #region Constructors

        public NullableDoubleConverter()
            : base(new DoubleConverter())
        {
        }

        public NullableDoubleConverter(IFormatProvider formatProvider)
            : base(new DoubleConverter(formatProvider))
        {
        }

        public NullableDoubleConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new DoubleConverter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}