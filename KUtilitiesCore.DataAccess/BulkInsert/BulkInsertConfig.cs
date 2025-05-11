using System;
using System.Linq;

namespace KUtilitiesCore.DataAccess.BulkInsert
{
    /// <summary>
    /// Configuración para el proceso de inserción masiva de datos.
    /// </summary>
    public class BulkInsertConfig
    {
        public IConnectionBuilder ConnectionString { get; set; }
        public string DestinationTableName { get; set; }
        public int BatchSize { get; set; } = 1000;
        public int BulkCopyTimeout { get; set; } = 600;
        public Dictionary<string, string> ColumnMappings { get; set; } = [];
        public BulkInsertConfig(IConnectionBuilder connectionBuilder,string destinationTableName)
        {
            if (connectionBuilder == null)
                throw new ArgumentNullException(nameof(connectionBuilder));
            if (destinationTableName == null) 
                throw new ArgumentNullException(nameof(destinationTableName));
            ConnectionString = connectionBuilder;
            DestinationTableName = destinationTableName;
        }
    }
}
