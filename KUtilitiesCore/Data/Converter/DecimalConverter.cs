using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class DecimalConverter : NonNullableConverter<Decimal>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public DecimalConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public DecimalConverter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Number)
        {
        }

        public DecimalConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Decimal result)
        {
            if (Decimal.TryParse(value, numberStyles, formatProvider, out result))
                return true;
            if (value.ToLowerInvariant().Contains("e"))
            {
                double parseDouble;
                if (double.TryParse(value, out parseDouble))
                {
                    result = Convert.ToDecimal(parseDouble);
                    return true;
                }
            }
            return false;
        }

        #endregion Methods
    }
}