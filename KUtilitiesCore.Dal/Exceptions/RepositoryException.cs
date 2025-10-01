namespace KUtilitiesCore.Dal.Exceptions
{
    public class RepositoryException : DataAccessException
    {
        #region Constructors

        public RepositoryException()
        { }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion Constructors
    }
}