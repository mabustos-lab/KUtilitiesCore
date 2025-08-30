namespace KUtilitiesCore.DataAccess.Helpers
{
    public interface IDataReaderConverter
    {
        /// <summary>
        /// Indica si el convertidor puede realizar conversiones
        /// </summary>
        bool RequiredConvert { get; }
        /// <summary>
        /// Agrega una transformación de conjunto de resultados al convertidor.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <returns>La instancia actual de IDataReaderConverter.</returns>
        IDataReaderConverter WithResult<TResult>() where TResult : class,new();
    }
}