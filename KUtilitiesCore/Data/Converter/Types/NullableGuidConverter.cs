using KUtilitiesCore.Data.Converter.Abstracts;
using System;

namespace KUtilitiesCore.Data.Converter.Types
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