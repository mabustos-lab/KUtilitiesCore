using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Utils
{
    public class RepositoryException : DataAccessException
    {
        public RepositoryException() { }
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
