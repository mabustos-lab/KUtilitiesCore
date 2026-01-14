#if NET48

using SqlClient = System.Data.SqlClient;

#else
using SqlClient = Microsoft.Data.SqlClient;
#endif

namespace KUtilitiesCore.Dal.BulkInsert
{
    /// <summary>
    /// Configuración para el proceso de inserción masiva de datos.
    /// </summary>
    public class BulkOperationsConfig
    {
        #region Constructors

        public BulkOperationsConfig(string destinationTableName)
        {
            if (string.IsNullOrEmpty(destinationTableName))
                throw new ArgumentNullException(nameof(destinationTableName));

            DestinationTableName = destinationTableName;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Tamaño del lote para cada operación de inserción masiva. Indica cuántas filas se
        /// enviarán al servidor en cada lote.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Tiempo de espera (en segundos) para la operación de copia masiva. Si la operación excede
        /// este tiempo, se cancelará.
        /// </summary>
        public int BulkCopyTimeout { get; set; } = 600;

        /// <summary>
        /// Define el comportamiento de la carga masiva de datos en SQL Server.
        /// </summary>
        public SqlClient.SqlBulkCopyOptions Options { get; set; } = SqlClient.SqlBulkCopyOptions.Default;

        /// <summary>
        /// Diccionario para mapear columnas del DataTable de origen a columnas de la tabla destino.
        /// Clave: Nombre de la columna en el DataTable de origen.
        /// Valor: Nombre de la columna en la tabla de base de datos destino.
        /// </summary>
        public Dictionary<string, string> ColumnMappings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Nombre de la tabla destino en la base de datos.
        /// </summary>
        public string DestinationTableName { get; set; }

        /// <summary>
        /// (Para BulkUpdate/BulkDelete) Nombre de la columna que actúa como clave primaria o
        /// identificador único para las cláusulas WHERE.
        /// </summary>
        public string KeyColumnNameForUpdateDelete { get; set; }

        #endregion Properties
    }
}