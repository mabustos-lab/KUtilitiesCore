using System.Data;

namespace KUtilitiesCore.Tracking.Collection
{
    public static class TrackedStatusExtensions
    {
        /// <summary>
        /// Traduce un valor de TrackedStatus al correspondiente DataRowState.
        /// </summary>
        /// <param name="status">El estado de seguimiento.</param>
        /// <returns>El DataRowState equivalente.</returns>
        public static DataRowState ToDataRowState(this TrackedStatus status)
        {
            switch (status)
            {
                case TrackedStatus.Added:
                    return DataRowState.Added;
                case TrackedStatus.Modified:
                    return DataRowState.Modified;
                case TrackedStatus.Removed:
                    return DataRowState.Deleted;
                case TrackedStatus.UnModified:
                default:
                    return DataRowState.Unchanged;
            }
        }
    }
}
