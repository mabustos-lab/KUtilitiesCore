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
        /// Builds an IReaderResultSet by applying the configured transformations to the provided IDataReader.
        /// This method processes a single result set from the IDataReader and indicates if more result sets are available.
        /// </summary>
        /// <param name="reader">The IDataReader to translate.</param>
        /// <param name="moreResultSets">Outputs true if there are more result sets in the reader, false otherwise.</param>
        /// <returns>An IReaderResultSet containing the results of the *current* result set.</returns>
        IReaderResultSet BuildReaderResultSet(IDataReader reader, ref bool moreResultSets);
    }
}