using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableSByteConverter : NullableInnerConverter<SByte>
    {
        #region Constructors

        public NullableSByteConverter()
            : base(new SByteConverter())
        {
        }

        public NullableSByteConverter(IFormatProvider formatProvider)
            : base(new SByteConverter(formatProvider))
        {
        }

        public NullableSByteConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new SByteConverter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}