using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET48
using SqlConnectionAlias = System.Data.SqlClient.SqlConnection;
using SqlInfoMessageEventArgsAlias = System.Data.SqlClient.SqlInfoMessageEventArgs;
using SqlErrorAlias = System.Data.SqlClient.SqlError;
#else
using SqlConnectionAlias = Microsoft.Data.SqlClient.SqlConnection;
using SqlInfoMessageEventArgsAlias = Microsoft.Data.SqlClient.SqlInfoMessageEventArgs;
using SqlErrorAlias = Microsoft.Data.SqlClient.SqlError;
#endif

namespace KUtilitiesCore.Dal.SQLLog
{
    public class SqlConnectionLogger : IDisposable
    {
        private readonly SqlConnectionAlias _connection;
        private readonly Action<SqlLogEntry> _logAction;
        private readonly SqlLoggingOptions _options;
        private readonly string _server;
        private readonly string _database;
        private readonly string _connectionId;
        private readonly Stopwatch _connectionTimer;

        private SqlConnectionLogger(SqlConnectionAlias connection,
            Action<SqlLogEntry> logAction,
            SqlLoggingOptions options)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logAction = logAction;
            _options = options ?? new SqlLoggingOptions();

            _server = connection.DataSource;
            _database = connection.Database;
            _connectionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _connectionTimer = Stopwatch.StartNew();

            // Suscribir al evento
            connection.InfoMessage += OnInfoMessage;
            connection.StateChange += OnStateChange;
        }

        public static IDisposable Attach(SqlConnectionAlias connection,
            Action<SqlLogEntry> logAction,
            SqlLoggingOptions options = null)
        {
            return new SqlConnectionLogger(connection, logAction, options);
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgsAlias e)
        {
            try
            {
                foreach (SqlErrorAlias err in e.Errors)
                {
                    var entry = CreateLogEntry(err);

                    // Aplicar filtros
                    if (ShouldLog(entry))
                    {
                        _logAction?.Invoke(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback logging para errores en el logger
                Console.Error.WriteLine($"Error en SQL logging: {ex.Message}");
            }
        }

        private void OnStateChange(object sender, StateChangeEventArgs e)
        {
            if (_options.IncludeConnectionInfo && _logAction != null)
            {
                var entry = new SqlLogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    LogLevel = SqlLogLevel.Info,
                    Message = $"Conexión cambiada de {e.OriginalState} a {e.CurrentState}",
                    Server = _server,
                    Database = _database,
                    ConnectionId = _connectionId,
                    ExecutionTime = _connectionTimer.Elapsed
                };

                _logAction(entry);
            }
        }

        private SqlLogEntry CreateLogEntry(SqlErrorAlias error)
        {
            var logLevel = DetermineLogLevel(error);
            var message = error.Message;

            // Truncar mensajes muy largos
            if (_options.MaxMessageLength > 0 && message.Length > _options.MaxMessageLength)
            {
                message = message.Substring(0, _options.MaxMessageLength) + "... [TRUNCATED]";
            }

            return new SqlLogEntry
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel,
                Message = message,
                Procedure = error.Procedure,
                LineNumber = error.LineNumber,
                ErrorNumber = error.Number,
                Severity = error.Class,
                State = error.State,
                Server = _server,
                Database = _database,
                ConnectionId = _connectionId,
                ExecutionTime = _connectionTimer.Elapsed
            };
        }

        private SqlLogLevel DetermineLogLevel(SqlErrorAlias error)
        {
            // Clasificar según severidad de SQL Server
            if (error.Class == 0) return SqlLogLevel.Info;
            if (error.Class <= 10) return SqlLogLevel.Warning;
            if (error.Number == 0) return SqlLogLevel.Debug; // PRINT statements

            return SqlLogLevel.Error;
        }

        private bool ShouldLog(SqlLogEntry entry)
        {
            // 1. Filtro por nivel mínimo
            if (entry.LogLevel < _options.MinimumLogLevel)
                return false;

            // 2. Filtro por números de error ignorados
            if (_options.IgnoredErrorNumbers.Contains(entry.ErrorNumber))
                return false;

            // 3. Filtrar mensajes del sistema
            if (_options.FilterSystemMessages && IsSystemMessage(entry))
                return false;

            // 4. Filtro personalizado
            if (_options.CustomFilter != null && !_options.CustomFilter(entry))
                return false;

            return true;
        }

        private bool IsSystemMessage(SqlLogEntry entry)
        {
            // Filtrar mensajes comunes del sistema
            var systemMessages = new[]
            {
            "Changed database context to",
            "Changed language setting to",
            "The transaction ended"
        };

            return systemMessages.Any(m =>
                entry.Message.ToLower().Contains(m.ToLower()));
        }

        public void Dispose()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.InfoMessage -= OnInfoMessage;
                    _connection.StateChange -= OnStateChange;

                    if (_logAction != null && _options.IncludeConnectionInfo)
                    {
                        var entry = new SqlLogEntry
                        {
                            Timestamp = DateTime.UtcNow,
                            LogLevel = SqlLogLevel.Info,
                            Message = "SQL logging deshabilitado",
                            Server = _server,
                            Database = _database,
                            ConnectionId = _connectionId,
                            ExecutionTime = _connectionTimer.Elapsed
                        };

                        _logAction(entry);
                    }
                }
            }
            catch
            {
                // Ignorar errores durante la limpieza
            }
        }
    }

    public class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new NullDisposable();
        public void Dispose() { }
    }
}
