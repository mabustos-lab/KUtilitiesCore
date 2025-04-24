using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class UInt64Converter : NonNullableConverter<ulong>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public UInt64Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public UInt64Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public UInt64Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out UInt64 result)
        {
            return UInt64.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}