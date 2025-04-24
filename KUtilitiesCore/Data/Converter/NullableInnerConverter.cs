namespace KUtilitiesCore.Data.Converter
{
    public abstract class NullableInnerConverter<TTargetType> : NullableConverter<TTargetType?>
            where TTargetType : struct
    {
        #region Fields

        private readonly NonNullableConverter<TTargetType> internalConverter;

        #endregion Fields

        #region Constructors

        public NullableInnerConverter(NonNullableConverter<TTargetType> internalConverter)
        {
            this.internalConverter = internalConverter;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out TTargetType? result)
        {
            result = default(TTargetType?);

            TTargetType innerConverterResult;

            if (internalConverter.TryConvert(value, out innerConverterResult))
            {
                result = innerConverterResult;

                return true;
            }

            return false;
        }

        #endregion Methods
    }
}