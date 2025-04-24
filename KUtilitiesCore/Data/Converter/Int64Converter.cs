using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class Int64Converter : NonNullableConverter<long>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public Int64Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public Int64Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public Int64Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Int64 result)
        {
            return Int64.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}