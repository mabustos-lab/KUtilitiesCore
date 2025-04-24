using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class TimeSpanConverter : NonNullableConverter<TimeSpan>
    {
        #region Fields

        private readonly string format;
        private readonly IFormatProvider formatProvider;
        private readonly TimeSpanStyles timeSpanStyles;

        #endregion Fields

        #region Constructors

        public TimeSpanConverter()
            : this(string.Empty)
        {
        }

        public TimeSpanConverter(string format)
            : this(format, CultureInfo.InvariantCulture)
        {
        }

        public TimeSpanConverter(string format, IFormatProvider formatProvider)
            : this(format, formatProvider, TimeSpanStyles.None)
        {
        }

        public TimeSpanConverter(string format, IFormatProvider formatProvider, TimeSpanStyles timeSpanStyles)
        {
            this.format = format;
            this.formatProvider = formatProvider;
            this.timeSpanStyles = timeSpanStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out TimeSpan result)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return TimeSpan.TryParse(value, formatProvider, out result);
            }
            return TimeSpan.TryParseExact(value, format, formatProvider, timeSpanStyles, out result);
        }

        #endregion Methods
    }
}