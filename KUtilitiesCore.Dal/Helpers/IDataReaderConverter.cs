#nullable enable
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Interfaz fluida para configurar cómo se traducen los conjuntos de resultados de un
    /// IDataReader a objetos fuertemente tipados o DataTables.
    /// </summary>
    public interface IDataReaderConverter
    {
        /// <summary>
        /// Establece los prefijos de columna que se eliminarán al mapear las columnas a propiedades.
        /// </summary>
        /// <param name="args">Prefijos a remover de los nombres de columna antes del mapeo.</param>
        /// <returns>La instancia actual para encadenar configuraciones.</returns>
        IDataReaderConverter SetColumnPrefixesToRemove(params string[] args);
        /// <summary>
        /// Activa o desactiva el mapeo estricto, que falla si una propiedad no tiene columna correspondiente.
        /// </summary>
        /// <param name="value">true para activar el mapeo estricto; false en caso contrario.</param>
        /// <returns>La instancia actual para encadenar configuraciones.</returns>
        IDataReaderConverter SetStrictMapping(bool value);
        /// <summary>
        /// Agrega una estrategia que traduce los conjuntos de resultados no configurados
        /// explícitamente a su representación como DataTable.
        /// </summary>
        /// <returns>La instancia actual para encadenar configuraciones.</returns>
        IDataReaderConverter WithDefaultDataTable();
        /// <summary>
        /// Configura el mapeo del conjunto de resultados actual a una colección de objetos del tipo
        /// <typeparamref name="TResult"/> mediante reflexión. Requiere un tipo con constructor sin parámetros.
        /// </summary>
        /// <typeparam name="TResult">El tipo de objeto al que se mapeará cada fila del conjunto de resultados.</typeparam>
        /// <returns>La instancia actual para encadenar configuraciones.</returns>
        IDataReaderConverter WithResult<TResult>() where TResult : class, new();

        /// <summary>
        /// Configura el mapeo del conjunto de resultados actual a una colección de objetos del tipo <typeparamref name="TResult"/>
        /// utilizando un delegado personalizado. Útil para mapear a structs, records o cualquier tipo sin constructor sin parámetros.
        /// </summary>
        /// <typeparam name="TResult">El tipo de objeto al que se mapeará cada fila del conjunto de resultados.</typeparam>
        /// <param name="mapper">Función que recibe un DataTable y retorna una colección de objetos del tipo <typeparamref name="TResult"/>.</param>
        /// <returns>La instancia actual de <see cref="IDataReaderConverter"/> para encadenar configuraciones.</returns>
        IDataReaderConverter WithResult<TResult>(Func<DataTable, IEnumerable<TResult>> mapper);

        /// <summary>
        /// Crea un IReaderResultSet aplicando las transformaciones configuradas al IDataReader proporcionado.
        /// Este método procesa un único conjunto de resultados del IDataReader e indica si hay más conjuntos de resultados disponibles.
        /// </summary>
        /// <param name="reader">El IDataReader que se va a traducir.</param>
        /// <param name="moreResultSets">Devuelve true si hay más conjuntos de resultados en el lector, false en caso contrario.</param>
        /// <returns>Un IReaderResultSet que contiene los resultados del conjunto de resultados *actual*.</returns>
        IReaderResultSet BuildReaderResultSet(IDataReader reader, ref bool moreResultSets);
    }
}