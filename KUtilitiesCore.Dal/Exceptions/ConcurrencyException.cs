namespace KUtilitiesCore.Dal.Exceptions
{
    public class ConcurrencyException : Exception
    {
        #region Constructors

        public ConcurrencyException() : base("Se detectó un conflicto de concurrencia...")
        {
        }

        public ConcurrencyException(string message) : base(message)
        {
        }

        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion Constructors
    }
}