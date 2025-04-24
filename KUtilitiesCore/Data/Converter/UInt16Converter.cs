using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class UInt16Converter : NonNullableConverter<ushort>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public UInt16Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public UInt16Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public UInt16Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out UInt16 result)
        {
            return UInt16.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}