using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableDateTimeConverter : NullableInnerConverter<DateTime>
    {
        #region Constructors

        public NullableDateTimeConverter()
            : base(new DateTimeConverter())
        {
        }

        public NullableDateTimeConverter(string dateTimeFormat)
            : base(new DateTimeConverter(dateTimeFormat))
        {
        }

        public NullableDateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider)
            : base(new DateTimeConverter(dateTimeFormat, formatProvider))
        {
        }

        public NullableDateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider, DateTimeStyles dateTimeStyles)
            : base(new DateTimeConverter(dateTimeFormat, formatProvider, dateTimeStyles))
        {
        }

        #endregion Constructors
    }
}