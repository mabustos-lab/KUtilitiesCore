
namespace KUtilitiesCore.DataAccess.Helpers
{
    public interface IReaderResultSet
    {
        bool HasResultsets { get; }

        IEnumerable<TResult> GetResult<TResult>(int index = 0);
    }
}