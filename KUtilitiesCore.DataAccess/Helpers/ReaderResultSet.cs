using System.Collections;

namespace KUtilitiesCore.DataAccess.Helpers
{
    /// <summary>
    /// Representa una colección de conjuntos de resultados recuperados de un lector de datos.
    /// </summary>
    internal sealed class ReaderResultSet : IReaderResultSet
    {
        #region Fields

        private readonly List<IEnumerable> resultSets;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase ReaderResultSet.
        /// </summary>
        public ReaderResultSet()
        {
            resultSets = new List<IEnumerable>();
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public bool HasResultsets => resultSets.Count > 0;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public IEnumerable<TResult> GetResult<TResult>()
        {
            return resultSets.OfType<TResult>();
        }

        /// <summary>
        /// Agrega un conjunto de resultados a la colección.
        /// </summary>
        /// <param name="value">El conjunto de resultados a agregar.</param>
        internal void AddResult(IEnumerable value)
        {
            resultSets.Add(value);
        }

        #endregion Methods
    }
}