using System.Data;
using System.Collections;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Represents a strategy for mapping an IDataReader's current result set to a specific type.
    /// </summary>
    internal interface IMappingStrategy
    {
        /// <summary>
        /// Maps the provided DataTable (representing a single result set) to an object.
        /// </summary>
        /// <param name="dataTable">The DataTable containing the result set data.</param>
        /// <returns>An object representing the mapped result set (e.g., List<T>).</returns>
        object Map(DataTable dataTable);
    }
}
