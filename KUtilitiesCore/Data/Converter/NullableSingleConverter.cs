using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableSingleConverter : NullableInnerConverter<float>
    {
        #region Constructors

        public NullableSingleConverter()
            : base(new SingleConverter())
        {
        }

        public NullableSingleConverter(IFormatProvider formatProvider)
            : base(new SingleConverter(formatProvider))
        {
        }

        public NullableSingleConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new SingleConverter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}