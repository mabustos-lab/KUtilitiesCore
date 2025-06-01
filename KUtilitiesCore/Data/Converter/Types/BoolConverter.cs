using KUtilitiesCore.Data.Converter.Abstracts;
using System;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class BoolConverter : NonNullableConverter<bool>
    {
        #region Fields

        private readonly StringComparison stringComparism;
        private string falseValue;
        private string trueValue;

        #endregion Fields

        #region Properties

        public string FalseValue
        {
            get => trueValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("El valor de false no puede ser nulo ni vacío.");
                falseValue = value;
            }
        }

        public string TrueValue
        {
            get => trueValue;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("El valor de true no puede ser nulo ni vacío.");
                trueValue = value;
            }
        }

        #endregion Properties

        #region Constructors

        public BoolConverter()
            : this("true", "false", StringComparison.OrdinalIgnoreCase) { }

        public BoolConverter(string trueValue, string falseValue, StringComparison stringComparism)
        {
            if (string.IsNullOrEmpty(trueValue))
                throw new ArgumentException("El valor de true no puede ser nulo ni vacío.");
            if (string.IsNullOrEmpty(falseValue))
                throw new ArgumentException("El valor de false no puede ser nulo ni vacío.");
            this.trueValue = trueValue;
            this.falseValue = falseValue;
            this.stringComparism = stringComparism;
        }

        #endregion Constructors

        #region Methods

        protected override bool InternalConvert(string value, out bool result)
        {
            result = false;

            if (string.Equals(trueValue, value, stringComparism))
            {
                result = true;

                return true;
            }

            if (string.Equals(falseValue, value, stringComparism))
            {
                result = false;

                return true;
            }

            return false;
        }

        #endregion Methods
    }
}