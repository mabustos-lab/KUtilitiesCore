using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableSByteConverter : NullableInnerConverter<sbyte>
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