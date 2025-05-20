namespace KUtilitiesCore.Data.Converter.Abstracts
{
    public abstract class NullableConverter<TTargetType> : BaseConverter<TTargetType>
    {
        #region Methods

        public override bool TryConvert(string value, out TTargetType result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default!;

                return true;
            }

            return InternalConvert(value, out result);
        }

        protected abstract bool InternalConvert(string value, out TTargetType result);

        #endregion Methods
    }
}