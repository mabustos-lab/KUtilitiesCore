using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    public class UnregisteredRepositoryException:Exception
    {
        public UnregisteredRepositoryException(string message) : base(message) { }
        public UnregisteredRepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
