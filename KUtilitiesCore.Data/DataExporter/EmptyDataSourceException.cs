using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataExporter
{
    [Serializable]
    public class EmptyDataSourceException : Exception
    {
        public EmptyDataSourceException() { }

        public EmptyDataSourceException(string message) : base(message) { }

        public EmptyDataSourceException(string message, Exception innerException) : base(message, innerException) { }

        protected EmptyDataSourceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
