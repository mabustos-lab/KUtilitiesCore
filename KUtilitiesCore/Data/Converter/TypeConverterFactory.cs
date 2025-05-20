using System;

namespace KUtilitiesCore.Data.Converter
{
    public class TypeConverterFactory
    {
        #region Fields

        private static readonly Lazy<TypeConverterFactory> instance = new Lazy<TypeConverterFactory>(() => new TypeConverterFactory());
        private readonly ITypeConverterProvider provider;

        #endregion Fields

        #region Constructors

        private TypeConverterFactory()
        {
            provider = new TypeConverterProvider();
        }

        #endregion Constructors

        #region Properties

        public static ITypeConverterProvider Provider => instance.Value.provider;

        #endregion Properties
    }
}