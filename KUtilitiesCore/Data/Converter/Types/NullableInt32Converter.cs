using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableInt32Converter : NullableInnerConverter<int>
    {
        #region Constructors

        public NullableInt32Converter()
            : base(new Int32Converter())
        {
        }

        public NullableInt32Converter(IFormatProvider formatProvider)
            : base(new Int32Converter(formatProvider))
        {
        }

        public NullableInt32Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
            : base(new Int32Converter(formatProvider, numberStyles))
        {
        }

        #endregion Constructors
    }
}