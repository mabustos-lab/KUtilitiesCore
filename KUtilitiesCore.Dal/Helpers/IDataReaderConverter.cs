#nullable enable
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    public interface IDataReaderConverter
    {
        IDataReaderConverter SetColumnPrefixesToRemove(params string[] args);
        IDataReaderConverter SetStrictMapping(bool value);
        IDataReaderConverter WithDefaultDataTable();
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