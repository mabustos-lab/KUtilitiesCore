using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Interfaz que representa una colección de conjuntos de resultados recuperados de un lector de datos.
    /// </summary>
    public interface IReaderResultSet
    {
        /// <summary>
        /// Parametros usados para obtener el resulset.
        /// </summary>
        /// <remarks>Todos los parametros motrados tanto de entrada como salda.</remarks>
        public IReadOnlyDictionary<string, object> ParamsUsed { get; }
        /// <summary>
        /// Indica si la colección de conjuntos de resultados contiene algún conjunto de resultados.
        /// </summary>
        bool HasResultsets { get; }

        /// <summary>
        /// Obtiene el número de conjuntos de resultados disponibles
        /// </summary>
        int ResultSetCount { get; }

        /// <summary>
        /// Recupera un conjunto de resultados fuertemente tipado por índice.
        /// </summary>
        /// <typeparam name="TResult">El tipo del conjunto de resultados.</typeparam>
        /// <param name="index">Índice del conjunto de resultados (0-based)</param>
        /// <returns>Un enumerable del tipo especificado.</returns>
        IEnumerable<TResult> GetResult<TResult>(int index = 0) where TResult : class, new();

        /// <summary>
        /// Recupera un DataTable por índice.
        /// </summary>
        /// <param name="index">Índice del conjunto de resultados (0-based)</param>
        /// <returns>DataTable con los datos del conjunto de resultados.</returns>
        DataTable GetDataTable(int index = 0);

        /// <summary>
        /// Recupera todos los conjuntos de resultados como DataTables
        /// </summary>
        /// <returns>Array de DataTables</returns>
        DataTable[] GetAllDataTables();
    }
}