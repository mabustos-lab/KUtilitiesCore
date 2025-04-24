using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class SByteConverter : NonNullableConverter<sbyte>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public SByteConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public SByteConverter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public SByteConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out sbyte result)
        {
            return sbyte.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}