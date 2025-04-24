using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class Int16Converter : NonNullableConverter<short>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public Int16Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public Int16Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public Int16Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Int16 result)
        {
            return Int16.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}