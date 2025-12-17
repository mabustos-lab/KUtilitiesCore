using KUtilitiesCore.Dal;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using KUtilitiesCore.Telemetry;
using System.Diagnostics;
using KUtilitiesCore.Dal.Exceptions;

#if NETFRAMEWORK
using System.Data.Entity;
#elif NETCOREAPP
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
#endif

namespace KUtilitiesCore.DataAccess.DALEfCore
{
    /// <summary>
    /// Implementación concreta de <see cref="IEfCoreContext"/> que proporciona acceso a
    /// funcionalidades de Entity Framework (EF6 y EF Core) junto con ejecución SQL mejorada.
    /// </summary>
    /// <remarks>
    /// Esta clase implementa el patrón Adapter para exponer un DbContext de Entity Framework como
    /// un contexto compatible con las interfaces de acceso a datos unificadas, incluyendo:
    /// 
    /// - Gestión robusta de transacciones
    /// - Ejecución SQL con resiliencia y métricas
    /// - Pool de comandos para mejor performance
    /// - Logging y telemetría integrados
    /// - Manejo mejorado de recursos y disposición
    /// 
    /// La clase maneja automáticamente las diferencias entre EF6 y EF Core mediante directivas
    /// de preprocesador para compilación condicional.
    /// </remarks>
    public sealed class EfCoreContext : IEfCoreContext
    {
        #region Fields

        private readonly DbContext _context;
        private readonly ILogger<EfCoreContext> _logger;
        private readonly IMetrics _metrics;
        private readonly ConcurrentBag<DbCommand> _commandPool;
        private readonly int _maxPoolSize = 20;
        private TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
        private bool _disposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia del contexto EF con las dependencias opcionales de logging y métricas.
        /// </summary>
        /// <param name="context">Instancia de DbContext (EF6 o EF Core). No puede ser nulo.</param>
        /// <param name="logger">Instancia de logger para registro de eventos. Opcional.</param>
        /// <param name="metrics">Instancia para seguimiento de métricas. Opcional.</param>
        /// <exception cref="ArgumentNullException">
        /// Se lanza cuando <paramref name="context"/> es <see langword="null"/>.
        /// </exception>
        public EfCoreContext(DbContext context, ILogger<EfCoreContext> logger = null, IMetrics metrics = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _metrics = metrics ?? NullMetrics.Instance;
            _commandPool = new ConcurrentBag<DbCommand>();

            _logger?.LogInformation("EfCoreContext inicializado para DbContext: {DbContextType}", context.GetType().Name);
        }

        #endregion Constructors

        #region Implementación de IEfCoreContext

        /// <inheritdoc/>
        public DbContext Context
        {
            get
            {
                EnsureNotDisposed();
                return _context;
            }
        }

        #endregion

        #region Implementación de ISqlExecutorContext

        /// <inheritdoc/>
        public DbConnection Connection
        {
            get
            {
                EnsureNotDisposed();
#if NETFRAMEWORK
                // EF6: Acceso directo a la conexión
                var connection = _context.Database.Connection;
                EnsureConnectionOpen(connection);
                return connection;
#elif NETCOREAPP
                // EF Core: Obtiene la conexión a través de GetDbConnection()
                var connection = _context.Database.GetDbConnection();
                EnsureConnectionOpen(connection);
                return connection;
#endif
            }
        }

        /// <summary>
        /// Obtiene o establece el tiempo de espera predeterminado para los comandos de base de datos en segundos.
        /// </summary>
        /// <value>
        /// Tiempo de espera en segundos. Valor predeterminado: 30 segundos.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Se lanza cuando se establece un valor menor o igual a cero.
        /// </exception>
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
        /// <inheritdoc>
        public Action<ISqlExecutorContext> OnConnectionOpened { get; set; } 

        /// <inheritdoc/>
        public IDaoParameterCollection CreateParameterCollection()
        {
            EnsureNotDisposed();
            try
            {
                var factory = DbProviderFactories.GetFactory(Connection);
                var parameterCollection = new DaoParameterCollection(() => factory.CreateParameter());
                _logger?.LogDebug("Colección de parámetros creada exitosamente");
                return parameterCollection;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al crear colección de parámetros");
                throw new DataAccessException("Error al crear colección de parámetros", null, GetProviderName(), ex);
            }
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var command = CreateCommand(sql, parameters, commandType, transaction);
                int result = command.ExecuteNonQuery();

                stopwatch.Stop();
                _metrics.TrackExecutionTime("ExecuteNonQuery", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "Operation", "ExecuteNonQuery" },
                    { "Provider", GetProviderName() },
                    { "CommandType", commandType.ToString() }
                });

                _logger?.LogDebug("ExecuteNonQuery ejecutado: {FilasAfectadas} filas en {Tiempo}ms",
                    result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteNonQuery: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar comando no query", sql, GetProviderName(), ex);
            }
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteNonQueryAsync(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var command = CreateCommand(sql, parameters, commandType, transaction);
                int result = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();
                _metrics.TrackExecutionTime("ExecuteNonQueryAsync", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "Operation", "ExecuteNonQueryAsync" },
                    { "Provider", GetProviderName() },
                    { "CommandType", commandType.ToString() }
                });

                _logger?.LogDebug("ExecuteNonQueryAsync ejecutado: {FilasAfectadas} filas en {Tiempo}ms",
                    result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ExecuteNonQueryAsync: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar comando no query asíncrono", sql, GetProviderName(), ex);
            }
        }

        /// <inheritdoc/>
        public TResult Scalar<TResult>(string sql, IDaoParameterCollection parameters = null)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var command = CreateCommand(sql, parameters);
                var result = command.ExecuteScalar();
                TResult typedResult = result is DBNull ? default : (TResult)result;

                stopwatch.Stop();
                _metrics.TrackExecutionTime("Scalar", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "Operation", "Scalar" },
                    { "Provider", GetProviderName() },
                    { "ResultType", typeof(TResult).Name }
                });

                _logger?.LogDebug("Scalar ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return typedResult;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en Scalar: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar consulta escalar", sql, GetProviderName(), ex);
            }
        }

        /// <inheritdoc/>
        public async Task<TResult> ScalarAsync<TResult>(
            string sql,
            IDaoParameterCollection parameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var command = CreateCommand(sql, parameters);
                var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                TResult typedResult = result is DBNull or null ? default : (TResult)result;

                stopwatch.Stop();
                _metrics.TrackExecutionTime("ScalarAsync", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "Operation", "ScalarAsync" },
                    { "Provider", GetProviderName() },
                    { "ResultType", typeof(TResult).Name }
                });

                _logger?.LogDebug("ScalarAsync ejecutado en {Tiempo}ms", stopwatch.ElapsedMilliseconds);

                return typedResult;
            }
            catch (DbException ex)
            {
                stopwatch.Stop();
                _logger?.LogError(ex, "Error en ScalarAsync: {Sql}", sql);
                throw new DataAccessException("Error al ejecutar consulta escalar asíncrona", sql, GetProviderName(), ex);
            }
        }

        #endregion

        #region Implementación de IDalContext

        /// <inheritdoc/>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            EnsureNotDisposed();

            try
            {
#if NETFRAMEWORK
                // EF6: BeginTransaction retorna DbContextTransaction
                var efTransaction = _context.Database.BeginTransaction(isolationLevel);
                var transaction = new TransactionEF(efTransaction);
#elif NETCOREAPP
                // EF Core: BeginTransaction retorna IDbContextTransaction
                var efTransaction = _context.Database.BeginTransaction(isolationLevel);
                var transaction = new TransactionEF(efTransaction);
#endif

                _logger?.LogDebug("Transacción EF iniciada con nivel de aislamiento: {IsolationLevel}", isolationLevel);
                _metrics.IncrementCounter("TransactionsStarted", 1, new Dictionary<string, string>
                {
                    { "Provider", GetProviderName() },
                    { "IsolationLevel", isolationLevel.ToString() }
                });

                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al iniciar transacción EF con nivel: {IsolationLevel}", isolationLevel);
                throw new DataAccessException("Error al iniciar transacción de Entity Framework", null, GetProviderName(), ex);
            }
        }

        /// <inheritdoc/>
        public bool DatabaseExists()
        {
            EnsureNotDisposed();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                bool exists;
#if NETFRAMEWORK
                // EF6
                exists = _context.Database.Exists();
#else
                // EF Core
                exists = _context.Database.CanConnect();
#endif

                stopwatch.Stop();
                _metrics.TrackExecutionTime("DatabaseExists", stopwatch.ElapsedMilliseconds, new Dictionary<string, string>
                {
                    { "Provider", GetProviderName() },
                    { "Exists", exists.ToString() }
                });

                _logger?.LogDebug("Verificación de existencia de BD: {Existe} en {Tiempo}ms", exists, stopwatch.ElapsedMilliseconds);

                return exists;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogWarning(ex, "La base de datos no está disponible");
                _metrics.TrackException(ex, new Dictionary<string, string>
                {
                    { "Operation", "DatabaseExists" },
                    { "Provider", GetProviderName() }
                });

                return false;
            }
        }

        #endregion

        #region Implementación de IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos no administrados utilizados por la clase y opcionalmente libera los recursos administrados.
        /// </summary>
        /// <param name="disposing">
        /// Es <see langword="true"/> para liberar tanto recursos administrados como no administrados;
        /// es <see langword="false"/> para liberar únicamente recursos no administrados.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberar comandos del pool
                    while (_commandPool.TryTake(out DbCommand command))
                    {
                        try
                        {
                            command.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error al liberar comando del pool");
                        }
                    }

                    // Liberar contexto de EF
                    try
                    {
                        _context?.Dispose();
                        _logger?.LogDebug("DbContext de EF liberado correctamente");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error al liberar DbContext de EF");
                    }
                }

                _disposed = true;
                _logger?.LogInformation("EfCoreContext liberado correctamente");
            }
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Crea un comando de base de datos configurado correctamente con soporte para pooling.
        /// </summary>
        /// <param name="sql">Sentencia SQL o nombre del procedimiento almacenado.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando (Texto, StoredProcedure, etc.).</param>
        /// <param name="transaction">Transacción asociada al comando.</param>
        /// <returns>Comando de base de datos configurado.</returns>
        /// <exception cref="ArgumentException">
        /// Se lanza cuando <paramref name="sql"/> es nulo o vacío.
        /// </exception>
        private DbCommand CreateCommand(
            string sql,
            IDaoParameterCollection parameters = null,
            CommandType commandType = CommandType.Text,
            ITransaction transaction = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentException("La sentencia SQL no puede estar vacía o ser nula", nameof(sql));

            var command = GetPooledCommand();

            try
            {
                // Configurar transacción si existe
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

                command.CommandText = sql;
                command.CommandType = commandType;
                command.CommandTimeout = ConnectionTimeout;

                // Agregar parámetros si existen
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }

                _logger?.LogDebug("Comando EF creado: {CommandType} - {Sql}", commandType, sql);
                return command;
            }
            catch (Exception ex)
            {
                ReturnToPool(command);
                _logger?.LogError(ex, "Error al crear comando EF: {Sql}", sql);
                throw new DataAccessException("Error al crear comando de base de datos", sql, GetProviderName(), ex);
            }
        }

        /// <summary>
        /// Obtiene un comando del pool o crea uno nuevo si el pool está vacío.
        /// </summary>
        /// <returns>Instancia de <see cref="DbCommand"/>.</returns>
        private DbCommand GetPooledCommand()
        {
            if (_commandPool.TryTake(out DbCommand command))
            {
                command.Parameters.Clear();
                return command;
            }

            var newCommand = Connection.CreateCommand();
            return newCommand;
        }

        /// <summary>
        /// Devuelve un comando al pool para su reutilización o lo libera si el pool está lleno.
        /// </summary>
        /// <param name="command">Comando a devolver al pool.</param>
        private void ReturnToPool(DbCommand command)
        {
            if (command == null) return;

            if (_commandPool.Count < _maxPoolSize)
            {
                command.Parameters.Clear();
                _commandPool.Add(command);
            }
            else
            {
                command.Dispose();
            }
        }

        /// <summary>
        /// Asegura que la conexión esté abierta antes de su uso.
        /// </summary>
        /// <param name="connection">Conexión a verificar.</param>
        private void EnsureConnectionOpen(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                try
                {
                    connection.Open();
                    _logger?.LogDebug("Conexión de EF abierta exitosamente");
                    OnConnectionOpened?.Invoke(this);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error al abrir conexión de EF");
                    throw new DataAccessException("Error al abrir conexión de Entity Framework", null, GetProviderName(), ex);
                }
            }
        }

        /// <summary>
        /// Obtiene el nombre del proveedor de base de datos actual.
        /// </summary>
        /// <returns>Nombre del proveedor de base de datos.</returns>
        private string GetProviderName()
        {
            return Connection.GetType().Namespace ?? "Unknown";
        }

        /// <summary>
        /// Verifica que la instancia no haya sido liberada.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// Se lanza cuando se intenta usar una instancia ya liberada.
        /// </exception>
        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EfCoreContext), "El contexto de EF ha sido liberado y no puede ser utilizado");
            }
        }

        #endregion
    }
}