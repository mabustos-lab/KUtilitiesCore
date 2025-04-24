using System;

namespace KUtilitiesCore.Data.Converter.Types
{
    public class ArrayConverter<TTargetType> : IArrayTypeConverter<TTargetType[]>
    {
        #region Fields

        private readonly ITypeConverter<TTargetType> internalConverter;

        #endregion Fields

        #region Constructors

        public ArrayConverter(ITypeConverter<TTargetType> internalConverter)
        {
            this.internalConverter = internalConverter;
        }

        #endregion Constructors

        #region Properties

        public char Separator { get; set; } = ',';
        public Type TargetType => typeof(TTargetType[]);

        #endregion Properties

        #region Methods

        public virtual bool CanConvert(string value)
        {
            string[] values = value.Split(Separator);
            return TryConvert(values, out _);
        }

        public virtual object TryConvert(string[] values)
        {
            TTargetType[] result = new TTargetType[values.Length];
            if (TryConvert(values, out result))
            {
                return result as object;
            }
            return null;
        }

        public virtual bool TryConvert(string[] values, out TTargetType[] result)
        {
            result = new TTargetType[values.Length];

            for (int pos = 0; pos < values.Length; pos++)
            {
                if (!internalConverter.TryConvert(values[pos], out TTargetType element))
                    return false;

                result[pos] = element;
            }

            return true;
        }

        public virtual object TryConvert(string value)
        {
            return TryConvert(value.Split(Separator));
        }

        #endregion Methods
    }
}