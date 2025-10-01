namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Interfaz que representa una colección de conjuntos de resultados recuperados de un lector de datos.
    /// </summary>
    public interface IReaderResultSet
    {
        #region Properties

        /// <summary>
        /// Indica si la colección de conjuntos de resultados contiene algún conjunto de resultados.
        /// </summary>
        bool HasResultsets { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Recupera un conjunto de resultados fuertemente tipado por índice.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <returns>Un enumerable del tipo especificado.</returns>
        IEnumerable<TResult> GetResult<TResult>();

        #endregion Methods
    }
}