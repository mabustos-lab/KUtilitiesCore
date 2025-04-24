using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class DoubleConverter : NonNullableConverter<double>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public DoubleConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public DoubleConverter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Float | NumberStyles.AllowThousands)
        {
        }

        public DoubleConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Double result)
        {
            return Double.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}