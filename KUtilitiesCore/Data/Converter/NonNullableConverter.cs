namespace KUtilitiesCore.Data.Converter
{
    public abstract class NonNullableConverter<TTargetType> : BaseConverter<TTargetType>
    {
        #region Methods

        public override bool TryConvert(string value, out TTargetType result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default(TTargetType);

                return false;
            }

            return InternalConvert(value, out result);
        }

        protected abstract bool InternalConvert(string value, out TTargetType result);

        #endregion Methods
    }
}