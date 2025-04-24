using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter
{
    internal class ByteConverter : NonNullableConverter<byte>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public ByteConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public ByteConverter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public ByteConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Byte result)
        {
            return Byte.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}