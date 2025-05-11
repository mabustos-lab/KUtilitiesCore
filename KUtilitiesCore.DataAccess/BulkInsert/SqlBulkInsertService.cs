using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Data.SqlClient;


#if NET48
using SqlClient = System.Data.SqlClient;
#else
using SqlClient = Microsoft.Data.SqlClient;
#endif


namespace KUtilitiesCore.DataAccess.BulkInsert
{
    /// <summary>
    /// Implementación de IBulkInsertService para realizar inserciones masivas en SQL Server
    /// utilizando SqlBulkCopy y DbConnection. También soporta estrategias genéricas para otros
    /// proveedores de bases de datos.
    /// </summary>
    public class SqlBulkInsertService : IBulkInsertService
    {
        private readonly BulkInsertConfig _config;

        /// <summary>
        /// Constructor que inicializa la configuración para la inserción masiva.
        /// </summary>
        /// <param name="config">Configuración para la inserción masiva.</param>
        /// <exception cref="ArgumentNullException">Se lanza si la configuración es nula.</exception>
        public SqlBulkInsertService(BulkInsertConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la base de datos.
        /// </summary>
        /// <param name="dataTable">El DataTable que contiene los datos a insertar.</param>
        /// <exception cref="ArgumentException">Se lanza si el DataTable es nulo o está vacío.</exception>
        public void BulkInsert(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                throw new ArgumentException("El DataTable no puede estar vacío.");

            using DbConnection connection = CreateDbConnection();
            connection.Open();

            if (_config.ConnectionString.ProviderName is "System.Data.SqlClient" or "Microsoft.Data.SqlClient")
            {
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // SqlBulkCopy solo se ejecuta si el proveedor es SQL Server
                        using var bulkCopy = new SqlClient.SqlBulkCopy((SqlClient.SqlConnection)connection);

                        bulkCopy.DestinationTableName = _config.DestinationTableName;
                        bulkCopy.BatchSize = _config.BatchSize;
                        bulkCopy.BulkCopyTimeout = _config.BulkCopyTimeout;

                        // Configuración de mapeo de columnas
                        foreach (var mapping in _config.ColumnMappings)
                            bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);

                        // Inserta los datos en la tabla destino
                        bulkCopy.WriteToServer(dataTable);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; // Relanza la excepción para el manejo externo
                    }
                }
            }
            else
            {
                // Estrategia genérica para otros proveedores (ejemplo: INSERT con múltiples valores)
                BulkInsertGeneric(connection, dataTable);
            }
        }

        /// <summary>
        /// Crea y configura una conexión de base de datos utilizando el proveedor especificado.
        /// </summary>
        /// <returns>Una instancia de DbConnection configurada.</returns>
        private DbConnection CreateDbConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(_config.ConnectionString.ProviderName);
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = _config.ConnectionString.CnnString;
            return connection;
        }

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
    }
}
