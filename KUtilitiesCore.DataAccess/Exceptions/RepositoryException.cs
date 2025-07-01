using KUtilitiesCore.DataAccess.Utils;
using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Exceptions
{
    public class RepositoryException : DataAccessException
    {
        public RepositoryException() { }
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
