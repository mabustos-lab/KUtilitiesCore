using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class SingleConverter : NonNullableConverter<float>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public SingleConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public SingleConverter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Float | NumberStyles.AllowThousands)
        {
        }

        public SingleConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out float result)
        {
            return float.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}