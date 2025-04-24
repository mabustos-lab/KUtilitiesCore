using System;

namespace KUtilitiesCore.Data.Converter
{
    public class EnumConverter<TTargetType> : NonNullableConverter<TTargetType>
            where TTargetType : struct, IConvertible
    {
        #region Fields

        private readonly Type enumType;
        private readonly bool ignoreCase;

        #endregion Fields

        #region Constructors

        public EnumConverter()
            : this(true)
        {
        }

        public EnumConverter(bool ignoreCase)
        {
            if (!(typeof(TTargetType)).IsEnum)
            {
                throw new ArgumentException(string.Format("Type {0} is not a valid Enum", enumType));
            }
            this.enumType = typeof(TTargetType);
            this.ignoreCase = ignoreCase;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out TTargetType result)
        {
            int intValue = -1;
            if (int.TryParse(value, out intValue))
            {
                result = default(TTargetType);
                bool success = Enum.IsDefined(typeof(TTargetType), intValue);
                if (success)
                {
                    result = (TTargetType)Enum.ToObject(typeof(TTargetType), intValue);
                }
                return success;
            }
            return Enum.TryParse<TTargetType>(value, ignoreCase, out result);
        }

        #endregion Methods
    }
}