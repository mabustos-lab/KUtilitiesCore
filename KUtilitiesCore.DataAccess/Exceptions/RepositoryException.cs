using KUtilitiesCore.Dal.Exceptions;

namespace KUtilitiesCore.DataAccess.Exceptions
{
    public class RepositoryException : Exception
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