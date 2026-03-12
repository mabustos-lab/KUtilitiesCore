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
        /// Crea un IReaderResultSet aplicando las transformaciones configuradas al IDataReader proporcionado.
        /// Este método procesa un único conjunto de resultados del IDataReader e indica si hay más conjuntos de resultados disponibles.
        /// </summary>
        /// <param name="reader">El IDataReader que se va a traducir.</param>
        /// <param name="moreResultSets">Devuelve true si hay más conjuntos de resultados en el lector, false en caso contrario.</param>
        /// <returns>Un IReaderResultSet que contiene los resultados del conjunto de resultados *actual*.</returns>
        IReaderResultSet BuildReaderResultSet(IDataReader reader, ref bool moreResultSets);
    }
}