using System;
using System.Collections;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Helpers
{
    /// <summary>
    /// Representa una colección de conjuntos de resultados recuperados de un lector de datos.
    /// </summary>
    sealed class ReaderResultSet : IReaderResultSet
    {
        private readonly List<IEnumerable> resultSets;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ReaderResultSet.
        /// </summary>
        public ReaderResultSet()
        {
            resultSets = new List<IEnumerable>();
        }

        /// <inheritdoc/>
        public bool HasResultsets => resultSets.Count > 0;

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

    }
}