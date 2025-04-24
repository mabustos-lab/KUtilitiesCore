using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class Int32Converter : NonNullableConverter<int>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public Int32Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public Int32Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public Int32Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Int32 result)
        {
            return Int32.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}