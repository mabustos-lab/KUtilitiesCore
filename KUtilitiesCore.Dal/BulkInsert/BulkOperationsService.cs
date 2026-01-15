using System.Data.Common;
using System.Data;
using KUtilitiesCore.Dal.ConnectionBuilder;
using Microsoft.Data.SqlClient;
using System.Reflection;
using KUtilitiesCore.Extensions;

#if NET48

using SqlClient = System.Data.SqlClient;

#else
using SqlClient = Microsoft.Data.SqlClient;
#endif

namespace KUtilitiesCore.Dal.BulkInsert
{
    /// <summary>
    /// Implementación de <see cref="IBulkOperationsService"/> para realizar inserciones masivas en
    /// SQL Server utilizando <see cref="SqlClient.SqlBulkCopy"/> y <see cref="DbConnection"/>.
    /// También soporta estrategias genéricas para otros proveedores de bases de datos.
    /// </summary>
    /// <remarks>Constructor que inicializa la configuración para la inserción masiva.</remarks>
    /// <param name="config">Configuración para la inserción masiva.</param>
    ///// <param name="connectionString">Encapsulación de la cadena de conección</param>
    /// <exception cref="ArgumentNullException">Se lanza si la configuración es nula.</exception>
    public class BulkOperationsService(BulkOperationsConfig config) : IBulkOperationsService
    {
        private readonly BulkOperationsConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la base de datos.
        /// </summary>
        /// <param name="dataTable">El DataTable que contiene los datos a insertar.</param>
        /// <param name="context">Contexto de conexión a la base de datos</param>
        /// <param name="externalTransaction">
        /// Transacción asociada a la operación, si no se asigna internamente se crea una transacción
        /// </param>
        public void BulkCopy<TContext>(DataTable dataTable, TContext context, ITransaction externalTransaction = null)
            where TContext : ISqlExecutorContext
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;
            ExecuteBulkOperation(context, externalTransaction, (bulkCopy) =>
            {
                ConfigureDefaultColumnMapping(bulkCopy);
                ConfigureColumnMapping(bulkCopy, dataTable);
                bulkCopy.WriteToServer(dataTable);
            });
        }

        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la base de datos.
        /// </summary>
        /// <param name="reader">El IDataReader que contiene los datos a insertar.</param>
        /// <param name="context">Contexto de conexión a la base de datos</param>
        /// <param name="externalTransaction">
        /// Transacción asociada a la operación, si no se asigna internamente se crea una transacción
        /// </param>
        public void BulkCopy<TContext>(IDataReader reader, TContext context, ITransaction externalTransaction = null)
            where TContext : ISqlExecutorContext
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            ExecuteBulkOperation(context, externalTransaction, (bulkCopy) =>
            {
                // Para IDataReader, si las columnas coinciden por nombre, a veces no es necesario
                // mapear, pero es buena práctica hacerlo si conocemos la estructura.
                ConfigureDefaultColumnMapping(bulkCopy);
                ConfigureColumnMapping(bulkCopy, reader);
                bulkCopy.WriteToServer(reader);
            });
        }
        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la base de datos.
        /// </summary>
        /// <param name="items">El IEnumerable<T> que contiene los datos a insertar.</param>
        /// <param name="context">Contexto de conexión a la base de datos</param>
        /// <param name="externalTransaction">
        /// Transacción asociada a la operación, si no se asigna internamente se crea una transacción
        /// </param>
        public void BulkInsert<T, TContext>(IEnumerable<T> items, TContext context, ITransaction externalTransaction = null)
           where TContext : ISqlExecutorContext
        {
            if (items == null) return;

            // Usamos el adaptador (definido en el siguiente archivo)
            using (var reader = new ObjectDataReader<T>(items))
            {
                ExecuteBulkOperation(context, externalTransaction, (bulkCopy) =>
                {
                    ConfigureDefaultColumnMapping(bulkCopy);
                    ConfigureColumnMapping<T>(bulkCopy);
                    bulkCopy.WriteToServer(reader);
                });
            }
        }

        ///// <summary>
        ///// Realiza una inserción masiva de datos desde un DataTable a la base de datos.
        ///// </summary>
        ///// <param name="dataTable">El DataTable que contiene los datos a insertar.</param>
        ///// <param name="context">Contexto de conexión a la base de datos</param>
        ///// <param name="externalTransaction">Transacción asociada a la operación, si no se asigna internamente se crea una transacción</param>
        ///// <exception cref="ArgumentException">Se lanza si el DataTable es nulo o está vacío.</exception>
        //public void BulkCopy<TContext>(DataTable dataTable, TContext context, ITransaction externalTransaction = null)
        //    where TContext : ISqlExecutorContext
        //{
        //    if (dataTable == null || dataTable.Rows.Count == 0)
        //        throw new ArgumentException("El DataTable no puede estar vacío.");

        // bool isLocalTransaction = (externalTransaction == null);

        // if (context.ProviderName is "System.Data.SqlClient" or "Microsoft.Data.SqlClient") {
        // ITransaction transaction = externalTransaction ?? context.BeginTransaction(); try { //
        // SqlBulkCopy solo se ejecuta si el proveedor es SQL Server using var bulkCopy = new
        // SqlClient.SqlBulkCopy((SqlClient.SqlConnection)context.Connection, _config.Options, (SqlClient.SqlTransaction)((TransactionBase)transaction).GetTransactionObject());

        // bulkCopy.DestinationTableName = _config.DestinationTableName; bulkCopy.BatchSize =
        // _config.BatchSize; bulkCopy.BulkCopyTimeout = _config.BulkCopyTimeout;

        // // Configuración de Mapeo de Columnas // Si el diccionario tiene datos, usamos el mapeo
        // explícito. // Si está vacío, asume que los nombres de columnas son idénticos. if
        // (_config.ColumnMappings.Count > 0) { // Configuración de mapeo de columnas foreach(var
        // mapping in _config.ColumnMappings) bulkCopy.ColumnMappings.Add(mapping.Key,
        // mapping.Value); } else { // Mapeo automático simple (opcional, por seguridad se
        // recomienda mapeo explícito) foreach (DataColumn col in dataTable.Columns) {
        // bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName); } }

        // // Inserta los datos en la tabla destino bulkCopy.WriteToServer(dataTable); // Si la
        // transacción fue creada localmente, hacemos Commit if (isLocalTransaction) {
        // transaction.Commit(); } } catch (Exception) { // Si falla y la transacción es local,
        // hacemos Rollback if (isLocalTransaction) { transaction.Rollback(); } throw; // Relanza la
        // excepción para el manejo externo } finally { // Nota: No cerramos la conexión aquí porque
        // fue inyectada (conn). // El que llama al método es responsable de la conexión, // pero sí
        // limpiamos la transacción local si fue creada. if (isLocalTransaction && transaction !=
        // null) { transaction.Dispose(); } } }

        //    else
        //    {
        //        // Estrategia genérica para otros proveedores (ejemplo: INSERT con múltiples valores)
        //        BulkInsertGeneric(context.Connection, dataTable);
        //    }
        //}

        /// <summary>
        /// Realiza una inserción masiva genérica para proveedores que no soportan SqlBulkCopy.
        /// </summary>
        /// <param name="connection">Conexión a la base de datos.</param>
        /// <param name="dataTable">El DataTable que contiene los datos a insertar.</param>
        private void BulkInsertGeneric(DbConnection connection, DataTable dataTable)
        {
            using (DbTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;

                        // Construcción de la consulta de inserción
                        var columnNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                        var parameterNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select((c, i) => $"@param{i}"));

                        command.CommandText = $"INSERT INTO {_config.DestinationTableName} ({columnNames}) VALUES ({parameterNames})";

                        // Reutilización del comando para múltiples inserciones
                        foreach (DataRow row in dataTable.Rows)
                        {
                            command.Parameters.Clear(); // Limpiar parámetros antes de cada inserción

                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                DbParameter param = command.CreateParameter();
                                param.ParameterName = $"@param{i}";
                                param.Value = row[i] ?? DBNull.Value; // Manejar valores NULL
                                command.Parameters.Add(param);
                            }

                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw; // Relanza la excepción para el manejo externo
                }
            }
        }

        private void ExecuteBulkOperation<TContext>(TContext context, ITransaction externalTransaction, Action<SqlClient.SqlBulkCopy> operation)
            where TContext : ISqlExecutorContext
        {
            // 1. Obtener la conexión subyacente (asumiendo SQL Server)
            var dbConnection = context.Connection as SqlClient.SqlConnection;
            if (dbConnection == null)
            {
                throw new InvalidOperationException("BulkCopy requiere una conexión de SQL Server.");
            }

            // 2. Gestionar la apertura de la conexión
            bool connectionOpenedLocally = false;
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
                connectionOpenedLocally = true;
            }

            // 3. Obtener la transacción real de SQL
            ITransaction transaction = externalTransaction ?? context.BeginTransaction();

            try
            {
                // 4. Configurar SqlBulkCopyOptions
                var options = SqlClient.SqlBulkCopyOptions.Default;
                if (_config.Options.HasFlag(SqlClient.SqlBulkCopyOptions.TableLock))
                    options |= SqlClient.SqlBulkCopyOptions.TableLock;
                if (_config.Options.HasFlag(SqlClient.SqlBulkCopyOptions.KeepIdentity))
                    options |= SqlClient.SqlBulkCopyOptions.KeepIdentity;

                using var bulkCopy = new SqlClient.SqlBulkCopy((SqlClient.SqlConnection)context.Connection, options,
                       (SqlClient.SqlTransaction)((TransactionBase)transaction).GetTransactionObject());
                {
                    bulkCopy.DestinationTableName = _config.DestinationTableName;
                    bulkCopy.BulkCopyTimeout = _config.BulkCopyTimeout;
                    bulkCopy.BatchSize = _config.BatchSize;

                    // 5. Ejecutar la operación inyectada
                    operation(bulkCopy);
                }
            }
            finally
            {
                // Solo cerramos si nosotros la abrimos y no hay transacción externa pendiente
                if (connectionOpenedLocally && externalTransaction == null)
                {
                    dbConnection.Close();
                }
            }
        }

        private void ConfigureDefaultColumnMapping(SqlClient.SqlBulkCopy bulkCopy)
        {
            // Limpiamos mapeos previos por seguridad
            bulkCopy.ColumnMappings.Clear();
            if (_config.ColumnMappings.Count > 0)
            {
                // Configuración de mapeo de columnas
                foreach (var mapping in _config.ColumnMappings)
                    bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
            }
        }

        /// <summary>
        /// Mapeo automático
        /// </summary>
        /// <param name="bulkCopy"></param>
        /// <param name="table"></param>
        private void ConfigureColumnMapping(SqlClient.SqlBulkCopy bulkCopy, DataTable table)
        {
            if (_config.ColumnMappings.Count > 0)
                return;

            foreach (DataColumn column in table.Columns)
            {
                // Mapeo simple: Nombre columna origen -> Nombre columna destino
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
        }

        /// <summary>
        /// Mapeo automático de acuerdo al Tipo
        /// </summary>
        private void ConfigureColumnMapping<T>(SqlClient.SqlBulkCopy bulkCopy)
        {
            if (_config.ColumnMappings.Count > 0)
                return;

            // Usamos reflexión para obtener las propiedades públicas de T
            // OPTIMIZACIÓN: Esto se podría cachear en un diccionario estático para evitar reflexión
            //               en cada llamada
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propName in properties.Select(x=>x.Name))
            {
                // Aquí podrías agregar lógica para ignorar propiedades con ciertos atributos [NotMapped]
                bulkCopy.ColumnMappings.Add(propName, propName);
            }
        }

        /// <summary>
        /// Mapeo automático del objeto IDataReader
        /// </summary>
        /// <param name="bulkCopy"></param>
        /// <param name="reader"></param>
        private void ConfigureColumnMapping(SqlClient.SqlBulkCopy bulkCopy, IDataReader reader)
        {
            if (_config.ColumnMappings.Count > 0)
                return;

            // Iteramos sobre las columnas disponibles en el Reader para mapearlas por nombre
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string colName = reader.GetName(i);
                if (!string.IsNullOrEmpty(colName))
                {
                    bulkCopy.ColumnMappings.Add(colName, colName);
                }
            }
        }
    }
}