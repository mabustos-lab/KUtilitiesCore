namespace KUtilitiesCore.Dal.Helpers
{
    public interface IDataReaderConverter
    {
        #region Properties

        /// <summary>
        /// Indica si el convertidor puede realizar conversiones
        /// </summary>
        bool RequiredConvert { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Agrega una transformación de conjunto de resultados al convertidor.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <returns>La instancia actual de IDataReaderConverter.</returns>
        IDataReaderConverter WithResult<TResult>() where TResult : class, new();

        #endregion Methods
    }
}