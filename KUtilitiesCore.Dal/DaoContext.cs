using KUtilitiesCore.Dal.ConnectionBuilder;
using KUtilitiesCore.Dal.Exceptions;
using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.Dal.SQLLog;
using KUtilitiesCore.Logger;
using KUtilitiesCore.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;


#if NET48

using SqlClient = System.Data.SqlClient;

#else
using SqlClient = Microsoft.Data.SqlClient;
#endif

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Proporciona un contexto centralizado para las operaciones de acceso a datos, incluyendo la
    /// creación de comandos, transacciones, gestión de conexiones a la base de datos y mecanismos
    /// de resiliencia.
    /// </summary>
    /// <remarks>
    /// Esta clase sirve como punto central para ejecutar operaciones de base de datos de manera
    /// robusta y eficiente. Incluye características como pool de comandos, reintentos automáticos
    /// para errores transitorios, telemetría integrada y gestión mejorada de recursos.
    /// </remarks>
    public class DaoContext : IDaoContext
    {
        #region Fields

        private readonly ConcurrentBag<DbCommand> _commandPool;

        private readonly IConnectionString _connectionString;

        private readonly DbProviderFactory _factory;

        private readonly ILogger<DaoContext> _logger;

        private readonly int _maxPoolSize = 20;

        private readonly IMetrics _metrics;

        private Lazy<DbConnection> _connection;

        private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        private bool _disposedValue;
        private IDisposable _sqlLoggerDisposable=null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DaoContext"/> con el constructor
        /// de conexión especificado.
        /// </summary>
        /// <param name="cnnStr">
        /// El constructor de conexión que proporciona la cadena de conexión y el nombre del
        /// proveedor para el acceso a la base de datos. Este parámetro no puede ser <see langword="null"/>.
        /// </param>
        /// <param name="logger">Instancia del logger para registrar eventos y errores. Opcional.</param>
        /// <param name="metrics">Instancia para el seguimiento de métricas y telemetría. Opcional.</param>
        /// <exception cref="ArgumentNullException">
        /// Se lanza cuando <paramref name="cnnStr"/> es <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Se lanza cuando no se puede crear la fábrica del proveedor de base de datos.
        /// </exception>
        public DaoContext(IConnectionString cnnStr, ILogger<DaoContext> logger = null, IMetrics metrics = null)
        {
            _connectionString = cnnStr ?? throw new ArgumentNullException(nameof(cnnStr));
            _logger = logger;
            _commandPool = new ConcurrentBag<DbCommand>();
            _metrics = metrics ?? NullMetrics.Instance;
            try
            {
                _factory = DbProviderFactories.GetFactory(cnnStr.ProviderName);
                _connection = new Lazy<DbConnection>(CreateConnection);
                _logger?.LogInformation("DaoContext inicializado para el proveedor: {ProviderName}", cnnStr.ProviderName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear DbProviderFactory para {ProviderName}", cnnStr.ProviderName);
                throw new ArgumentException($"No se pudo crear el proveedor de base de datos: {cnnStr.ProviderName}", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public DbConnection Connection
            => _connection.Value;

        /// <inheritdoc/>
        public int ConnectionTimeout
        {
            get { return (int)_defaultTimeout.TotalSeconds; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "El tiempo de espera debe ser mayor a cero");

                _defaultTimeout = TimeSpan.FromSeconds(value);
                _logger?.LogDebug("Timeout de conexión establecido en {Timeout} segundos", value);
            }
        }

        /// <inheritdoc/>
        public Action<ISqlExecutorContext> OnConnectionOpened { get; set; }

        /// <inheritdoc/>
        public string ProviderName => _connectionString.ProviderName;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            EnsureNotDisposed();
            try
            {
                var transaction = new TransactionBase(Connection.BeginTransaction(isolationLevel));
                _logger?.LogDebug("Transacción iniciada con nivel de aislamiento: {IsolationLevel}", isolationLevel);
                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al iniciar transacción con nivel de aislamiento: {IsolationLevel}", isolationLevel);
                throw new DataAccessException("Error al iniciar la transacción de base de datos", null, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public DbDataAdapter CreateAdapter(DbCommand command)
        {
            EnsureNotDisposed();
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            try
            {
                DbDataAdapter dataAdapter = _factory.CreateDataAdapter();
                dataAdapter.SelectCommand = command;
                return dataAdapter;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear adaptador de datos para el comando");
                throw new DataAccessException("Error al crear adaptador de datos", command.CommandText, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public DbCommand CreateCommand(string sql, IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            EnsureNotDisposed();

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("La sentencia SQL no puede estar vacía o ser nula", nameof(sql));

            DbCommand command = GetPooledCommand();

            try
            {
                // Validar compatibilidad de transacción
                if (transaction != null)
                {
                    var tx = ((TransactionBase)transaction).GetTransactionObject();
                    if (tx.Connection != Connection)
                    {
                        ReturnToPool(command);
                        throw new InvalidOperationException("La transacción pertenece a una conexión diferente");
                    }
                    command.Transaction = tx;
                }

                command.Connection = Connection;
                command.CommandText = sql;
                command.CommandType = commandType;
                command.CommandTimeout = (int)_defaultTimeout.TotalSeconds;

                if (parameters != null)
                {
                    foreach (DbParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                _logger?.LogDebug("Comando creado: {CommandType} - {Sql}", commandType, sql);
                return command;
            }
            catch (Exception ex)
            {
                ReturnToPool(command);
                _logger?.LogError(ex, "Error al crear comando: {Sql}", sql);
                throw new DataAccessException("Error al crear comando de base de datos", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public DbCommandBuilder CreateCommandBuilder()
        {
            EnsureNotDisposed();
            try
            {
                return _factory.CreateCommandBuilder();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear constructor de comandos");
                throw new DataAccessException("Error al crear constructor de comandos", null, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public IDaoParameterCollection CreateParameterCollection()
        {
            EnsureNotDisposed();
            return new DaoParameterCollection(CreateParameter);
        }

        /// <inheritdoc/>
        public bool DatabaseExists()
        {
            EnsureNotDisposed();

            if (_connection.IsValueCreated && _connection.Value.State == ConnectionState.Open)
                return true;

            try
            {
                using (var tempConnection = CreateConnection())
                {
                    return tempConnection.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "La base de datos no está disponible");
                return false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(string sql, IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text, ITransaction transaction = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters, commandType, transaction);
                int result = command.ExecuteNonQuery();

                stopwatch.Stop();
                _metrics.TrackMetric("NonQueryExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("ExecuteNonQuery ejecutado: {FilasAfectadas} filas afectadas en {Tiempo}ms",
                    result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteNonQuery: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar comando no query", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteNonQueryAsync(string sql, IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text, ITransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters, commandType, transaction);
                int result = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                _metrics.TrackMetric("NonQueryExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("ExecuteNonQueryAsync ejecutado: {FilasAfectadas} filas afectadas en {Tiempo}ms",
                    result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteNonQueryAsync: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar comando no query asíncrono", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public IReaderResultSet ExecuteReader(string sql, IDataReaderConverter translate,
            IDaoParameterCollection parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (translate == null)
                {
                    translate = DataReaderConverter.Create().WithDefaultDataTable();
                }
                IReaderResultSet result;
                using (DbCommand command = CreateCommand(sql, parameters, commandType))
                {
                    using DbDataReader reader = command.ExecuteReader();

                    result = translate.RequiredConvert
                        ? ((DataReaderConverter)translate).Translate(reader)
                        : new ReaderResultSet();
                }
                if(result is ReaderResultSet r1)
                    r1.SetParams(parameters);
                if (result is DataReaderConverter r2)
                    r2.SetParams(parameters);
                 
                stopwatch.Stop();
                _metrics.TrackMetric("ReaderExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("ExecuteReader ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteReader: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar reader", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IReaderResultSet> ExecuteReaderAsync(string sql, IDataReaderConverter translate,
            IDaoParameterCollection parameters = null, CommandType commandType = CommandType.StoredProcedure,
            CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();
            

            try
            {
                if (translate == null)
                {
                    translate = DataReaderConverter.Create().WithDefaultDataTable();
                }
                IReaderResultSet result;
                using(DbCommand command = CreateCommand(sql, parameters, commandType))
                {
                    using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken)
                        .ConfigureAwait(false);
                    result = translate.RequiredConvert
                        ? ((DataReaderConverter)translate).Translate(reader)
                        : new ReaderResultSet();
                }
                if (result is ReaderResultSet r1)
                    r1.SetParams(parameters);
                if (result is DataReaderConverter r2)
                    r2.SetParams(parameters);

                stopwatch.Stop();
                _metrics.TrackMetric("ReaderExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("ExecuteReaderAsync ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteReaderAsync: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar reader asíncrono", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public void FillDataSet(string sql, DataSet ds, string tableName, IDaoParameterCollection parameters = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters);
                using DbDataAdapter adapter = _factory.CreateDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(ds, tableName);

                stopwatch.Stop();
                _metrics.TrackMetric("FillDataSetExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("FillDataSet ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en FillDataSet: {Sql}", sql);
                throw new DataAccessException("Error al llenar DataSet", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public TResult Scalar<TResult>(string sql, IDaoParameterCollection parameters = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters);
                object result = command.ExecuteScalar();
                TResult typedResult = result is DBNull or null ? default : (TResult)result;

                stopwatch.Stop();
                _metrics.TrackMetric("ScalarExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("Scalar ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return typedResult;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en Scalar: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar consulta escalar", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TResult> ScalarAsync<TResult>(string sql, IDaoParameterCollection parameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters);
                object result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                TResult typedResult = result is DBNull or null ? default : (TResult)result;

                stopwatch.Stop();
                _metrics.TrackMetric("ScalarExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("ScalarAsync ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return typedResult;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ScalarAsync: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar consulta escalar asíncrona", sql, _connectionString.ProviderName, ex);
            }
        }

        /// <inheritdoc/>
        public int UpdateDataSet(DataSet ds, string selectCommandText, string tableName, ITransaction transaction = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(selectCommandText, null, CommandType.Text, transaction);
                using DbDataAdapter adapter = _factory.CreateDataAdapter();
                adapter.SelectCommand = command;

                using DbCommandBuilder builder = CreateCommandBuilder();
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();

                // Asignar transacción a los comandos generados
                if (transaction != null)
                {
                    var tx = ((TransactionBase)transaction).GetTransactionObject();
                    if (adapter.InsertCommand != null) adapter.InsertCommand.Transaction = tx;
                    if (adapter.UpdateCommand != null) adapter.UpdateCommand.Transaction = tx;
                    if (adapter.DeleteCommand != null) adapter.DeleteCommand.Transaction = tx;
                }

                int rowsAffected = adapter.Update(ds, tableName);

                stopwatch.Stop();
                _metrics.TrackMetric("UpdateDataSetExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("UpdateDataSet ejecutado: {FilasAfectadas} filas afectadas en {Tiempo}ms",
                    rowsAffected, stopwatch.ElapsedMilliseconds);

                return rowsAffected;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en UpdateDataSet para tabla: {TableName}", tableName);
                throw new DataAccessException("Error al actualizar DataSet", selectCommandText, _connectionString.ProviderName, ex);
            }
        }

        internal DbDataAdapter CreateDataAdapter()
        {
            EnsureNotDisposed();
            return _factory.CreateDataAdapter();
        }

        /// <summary>
        /// Libera los recursos no administrados utilizados por la clase y opcionalmente libera los
        /// recursos administrados.
        /// </summary>
        /// <param name="disposing">
        /// Es <see langword="true"/> para liberar tanto recursos administrados como no
        /// administrados; es <see langword="false"/> para liberar únicamente recursos no administrados.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sqlLoggerDisposable?.Dispose();
                    // Liberar conexión de forma segura
                    if (_connection != null && _connection.IsValueCreated)
                    {
                        try
                        {
                            if (_connection.Value.State != ConnectionState.Closed)
                            {
                                _connection.Value.Close();
                            }
                            _connection.Value.Dispose();
                            _logger?.LogDebug("Conexión de base de datos liberada");
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error al liberar la conexión de base de datos");
                        }
                    }

                    // Liberar comandos del pool con verificación de estado
                    int disposedCount = 0;
                    while (_commandPool.TryTake(out DbCommand command))
                    {
                        try
                        {
                            if (command.Connection?.State == ConnectionState.Open)
                            {
                                command.Cancel(); // Cancelar operaciones pendientes
                            }
                            command.Dispose();
                            disposedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error al liberar comando del pool");
                        }
                    }

                    _logger?.LogInformation("DaoContext liberado. {Count} comandos eliminados del pool",
                        disposedCount);
                }

                _connection = null;
                _disposedValue = true;
            }
        }

        private DbConnection CreateConnection()
        {
            try
            {
                DbConnection connection = _factory.CreateConnection();
                connection.ConnectionString = _connectionString.CnnString;
                connection.Open();
                _logger?.LogDebug("Conexión de base de datos creada y abierta");

                // Invocar delegado si está configurado
                OnConnectionOpened?.Invoke(this);

                EnableSqlLogging(connection);

                return connection;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear y abrir conexión de base de datos");
                throw new DataAccessException("Error al crear conexión de base de datos", null, _connectionString.ProviderName, ex);
            }
        }

        private void EnableSqlLogging(DbConnection connection)
        {
            if (_logger != null)
            {
                var options = new SqlLoggingOptions
                {
                    MinimumLogLevel = _logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)
                        ? SqlLogLevel.Debug
                        : SqlLogLevel.Warning,
                    FilterSystemMessages = true,
                    IgnoredErrorNumbers = new[] { 5701 } // Ignorar "Changed database context"
                };
                if (_logger is ILoggerService KLogger)
                {
                    _sqlLoggerDisposable = connection.EnableSqlLogging(
                        msg => KLogger.Log(
                            MapToLogLevel(msg.LogLevel),
                            Logger.CommonEventsID.DataAccess,
                            "[SQL] {Message} (Proc:{Procedure}:{Line}, Err:{ErrorNumber}:{Severity})",
                            msg.Message,
                            msg.Procedure,
                            msg.LineNumber,
                            msg.ErrorNumber,
                            msg.Severity),
                        options);
                }
                else
                {
                    _sqlLoggerDisposable = connection.EnableSqlLogging(
                        msg => _logger.Log(
                            MapToLogLevel(msg.LogLevel),
                            "[SQL] {Message} (Proc:{Procedure}:{Line}, Err:{ErrorNumber}:{Severity})",
                            msg.Message,
                            msg.Procedure,
                            msg.LineNumber,
                            msg.ErrorNumber,
                            msg.Severity),
                        options);
                }
            }
        }
        private static Microsoft.Extensions.Logging.LogLevel MapToLogLevel(SqlLogLevel sqlLevel)
        {
            return sqlLevel switch
            {
                SqlLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                SqlLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                SqlLogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
                SqlLogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                _ => Microsoft.Extensions.Logging.LogLevel.Information
            };
        }
        private DbParameter CreateParameter()
                            => _factory.CreateParameter();

        private void EnsureNotDisposed()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(DaoContext), "El contexto de acceso a datos ha sido liberado y no puede ser utilizado");
            }
        }

        private DbCommand GetPooledCommand()
        {
            if (_commandPool.TryTake(out DbCommand command))
            {
                // Verificar que el comando esté en estado válido
                if (command.Connection != null &&
                    command.Connection.State == ConnectionState.Open)
                {
                    _logger?.LogTrace("Comando reutilizado del pool");
                    return command;
                }
                else
                {
                    // Descargar comando con conexión inválida
                    command.Dispose();
                    _logger?.LogTrace("Comando descartado por conexión inválida");
                }
            }

            // Crear nuevo comando
            DbCommand newCommand = _factory.CreateCommand();
            newCommand.CommandTimeout = (int)_defaultTimeout.TotalSeconds;
            _logger?.LogTrace("Nuevo comando creado");

            return newCommand;
        }

        private void ReturnToPool(DbCommand command)
        {
            if (command == null) return;

            // Resetear completamente el comando
            ResetCommand(command);

            if (_commandPool.Count < _maxPoolSize &&
                command.Connection != null &&
                command.Connection.State == ConnectionState.Open)
            {
                _commandPool.Add(command);
                _logger?.LogTrace("Comando devuelto al pool. Tamaño actual: {Count}", _commandPool.Count);
            }
            else
            {
                command.Dispose();
                _logger?.LogTrace("Comando descartado, pool lleno o conexión cerrada");
            }
        }
        private void ResetCommand(DbCommand command)
        {
            if (command == null) return;

            // Resetear todas las propiedades a valores por defecto
            command.Parameters.Clear();
            command.CommandText = null;
            command.CommandType = CommandType.Text;
            command.Transaction = null;
            command.CommandTimeout = (int)_defaultTimeout.TotalSeconds;
            command.UpdatedRowSource = UpdateRowSource.Both;
            // Limpiar notificaciones (si aplica)
            if (command is SqlClient.SqlCommand sqlCommand)
            {
                sqlCommand.Notification = null;
            }
        }
        /// <inheritdoc/>
        public async Task FillDataSetAsync(string sql, DataSet ds, string tableName, IDaoParameterCollection parameters = null, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(sql, parameters);
                using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken)
                    .ConfigureAwait(false);

                DataTable dataTable = new DataTable(tableName);
                dataTable.Load(reader);
                ds.Tables.Add(dataTable);

                stopwatch.Stop();
                _metrics.TrackMetric("FillDataSetAsyncExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("FillDataSetAsync ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en FillDataSetAsync: {Sql}", sql);
                throw new DataAccessException("Error al llenar DataSet de forma asíncrona",
                    sql, _connectionString.ProviderName, ex);
            }
        }
        /// <inheritdoc/>
        public async Task<int> UpdateDataSetAsync(DataSet ds, string selectCommandText, string tableName, ITransaction transaction = null, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using DbCommand command = CreateCommand(selectCommandText, null,
                    CommandType.Text, transaction);
                using DbDataAdapter adapter = _factory.CreateDataAdapter();
                adapter.SelectCommand = command;

                using DbCommandBuilder builder = CreateCommandBuilder();

                // Generar comandos de forma asíncrona si es posible
                await Task.Run(() =>
                {
                    adapter.InsertCommand = builder.GetInsertCommand();
                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.DeleteCommand = builder.GetDeleteCommand();
                }, cancellationToken).ConfigureAwait(false);

                // Asignar transacción a los comandos generados
                if (transaction != null)
                {
                    var tx = ((TransactionBase)transaction).GetTransactionObject();
                    if (adapter.InsertCommand != null) adapter.InsertCommand.Transaction = tx;
                    if (adapter.UpdateCommand != null) adapter.UpdateCommand.Transaction = tx;
                    if (adapter.DeleteCommand != null) adapter.DeleteCommand.Transaction = tx;
                }

                // Ejecutar actualización de forma asíncrona
                int rowsAffected = await Task.Run(() => adapter.Update(ds, tableName),
                    cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                _metrics.TrackMetric("UpdateDataSetAsyncExecutionTime", stopwatch.ElapsedMilliseconds);
                _logger?.LogDebug("UpdateDataSetAsync ejecutado: {FilasAfectadas} filas en {Tiempo}ms",
                    rowsAffected, stopwatch.ElapsedMilliseconds);

                return rowsAffected;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en UpdateDataSetAsync para tabla: {TableName}", tableName);
                throw new DataAccessException("Error al actualizar DataSet de forma asíncrona",
                    selectCommandText, _connectionString.ProviderName, ex);
            }
        }

#endregion Methods
    }
}