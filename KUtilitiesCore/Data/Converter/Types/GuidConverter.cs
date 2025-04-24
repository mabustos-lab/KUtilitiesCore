using KUtilitiesCore.Data.Converter.Abstracts;
using System;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class GuidConverter : NonNullableConverter<Guid>
    {
        #region Fields

        private readonly string format;

        #endregion Fields

        #region Constructors

        public GuidConverter()
            : this(string.Empty)
        {
        }

        public GuidConverter(string format)
        {
            this.format = format;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out Guid result)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return Guid.TryParse(value, out result);
            }
            return Guid.TryParseExact(value, format, out result);
        }

        #endregion Methods
    }
}