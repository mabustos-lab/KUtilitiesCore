using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class NullableTimeSpanConverter : NullableInnerConverter<TimeSpan>
    {
        #region Constructors

        public NullableTimeSpanConverter()
            : base(new TimeSpanConverter())
        {
        }

        public NullableTimeSpanConverter(string format)
            : base(new TimeSpanConverter(format, CultureInfo.InvariantCulture))
        {
        }

        public NullableTimeSpanConverter(string format, IFormatProvider formatProvider)
            : base(new TimeSpanConverter(format, formatProvider, TimeSpanStyles.None))
        {
        }

        public NullableTimeSpanConverter(string format, IFormatProvider formatProvider, TimeSpanStyles timeSpanStyles)
            : base(new TimeSpanConverter(format, formatProvider, timeSpanStyles))
        {
        }

        #endregion Constructors
    }
}