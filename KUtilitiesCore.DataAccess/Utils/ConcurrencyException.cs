using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.Utils
{
    public class ConcurrencyException : DataAccessException
    {
        public ConcurrencyException() : base("Se detectó un conflicto de concurrencia...") { }
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
