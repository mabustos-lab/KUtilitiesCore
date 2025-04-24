using System;

namespace KUtilitiesCore.Data.Converter
{
    internal class NullableGuidConverter : NullableInnerConverter<Guid>
    {
        #region Constructors

        public NullableGuidConverter()
            : base(new GuidConverter())
        {
        }

        public NullableGuidConverter(string format)
            : base(new GuidConverter(format))
        {
        }

        #endregion Constructors
    }
}