using KUtilitiesCore.Logger.Helpers;
using KUtilitiesCore.Logger.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using KUtilitiesCore.Logger.Info;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de logger que escribe entradas en una tabla de SQL Server.
    /// </summary>
    /// <typeparam name="TCategoryName">Categoría del logger.</typeparam>
    public class SqlLogger<TCategoryName> : LoggerServiceAbstract<TCategoryName, SqlLoggerOptions>, ISqlLoggerService<TCategoryName>
    {
        private static bool _tableValidated = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SqlLogger{TCategoryName}"/>.
        /// </summary>
        /// <param name="options">Opciones de configuración.</param>
        public SqlLogger(SqlLoggerOptions options) : base(options)
        {
            if (options.AutoCreateTable)
            {
                EnsureDatabaseCreated();
            }
        }

        /// <inheritdoc/>
        internal override void WriteLog(LogEntry entry)
        {
            try
            {
                using (var connection = new SqlConnection(LogOptions.ConnectionString))
                {
                    connection.Open();
                    string query = $@"
                        INSERT INTO [{LogOptions.SchemaName}].[{LogOptions.TableName}] 
                        ([Timestamp], [LogLevel], [Category], [EventId], [Message], [Exception], [ApplicationName])
                        VALUES (@Timestamp, @LogLevel, @Category, @EventId, @Message, @Exception, @ApplicationName)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        object exception = (object)DBNull.Value;
                        if (entry.Exception != null)
                        {
                            ExceptionInfo ex = new ExceptionInfo(entry.Exception);
                            exception = ex.GetReport();
                        }
                        command.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                        command.Parameters.AddWithValue("@LogLevel", entry.Level.ToString());
                        command.Parameters.AddWithValue("@Category", CategoryName);
                        command.Parameters.AddWithValue("@EventId", entry.Event.Id);
                        command.Parameters.AddWithValue("@Message", entry.Message ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Exception", exception);
                        command.Parameters.AddWithValue("@ApplicationName", LogOptions.ApplicationName);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al escribir en el registro SQL: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void EnsureDatabaseCreated()
        {
            if (_tableValidated) return;

            lock (_lock)
            {
                if (_tableValidated) return;

                try
                {
                    using (var connection = new SqlConnection(LogOptions.ConnectionString))
                    {
                        connection.Open();
                        string createTableSql = $@"
                            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{LogOptions.SchemaName}].[{LogOptions.TableName}]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE [{LogOptions.SchemaName}].[{LogOptions.TableName}](
                                    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    [Timestamp] [datetime2](7) NOT NULL,
                                    [LogLevel] [nvarchar](50) NOT NULL,
                                    [Category] [nvarchar](255) NOT NULL,
                                    [EventId] [int] NULL,
                                    [Message] [nvarchar](max) NULL,
                                    [Exception] [nvarchar](max) NULL,
                                    [ApplicationName] [nvarchar](100) NULL
                                )
                            END";

                        using (var command = new SqlCommand(createTableSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        _tableValidated = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing SQL Logger table: {ex.Message}");
                }
            }
        }
    }
}
