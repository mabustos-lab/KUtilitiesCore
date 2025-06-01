using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class UInt32Converter : NonNullableConverter<uint>
    {
        #region Fields

        private readonly IFormatProvider formatProvider;
        private readonly NumberStyles numberStyles;

        #endregion Fields

        #region Constructors

        public UInt32Converter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public UInt32Converter(IFormatProvider formatProvider)
            : this(formatProvider, NumberStyles.Integer)
        {
        }

        public UInt32Converter(IFormatProvider formatProvider, NumberStyles numberStyles)
        {
            this.formatProvider = formatProvider;
            this.numberStyles = numberStyles;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out uint result)
        {
            return uint.TryParse(value, numberStyles, formatProvider, out result);
        }

        #endregion Methods
    }
}