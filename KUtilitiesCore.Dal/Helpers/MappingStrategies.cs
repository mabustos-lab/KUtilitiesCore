using KUtilitiesCore.Extensions;
using System.Collections;
using System.Data;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Strategy for mapping an IDataReader's current result set to a DataTable.
    /// </summary>
    internal class DataTableMappingStrategy : IMappingStrategy
    {
        public object Map(DataTable dataTable)
        {
            return dataTable;
        }
    }

    /// <summary>
    /// Strategy for mapping an IDataReader's current result set to a List of a specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to which the data reader's results should be mapped.</typeparam>
    internal class ObjectMappingStrategy<TResult> : IMappingStrategy where TResult : class, new()
    {
        private readonly TranslateOptions _options;

        public ObjectMappingStrategy(TranslateOptions options = null)
        {
            _options = options ?? new TranslateOptions();
        }

        public object Map(DataTable dataTable)
        {
            // Reusing the DataTableToEnumerable logic from ReaderResultSet
            return ReaderResultSet.DataTableToEnumerable<TResult>(dataTable, _options).ToList();
        }
    }
}
