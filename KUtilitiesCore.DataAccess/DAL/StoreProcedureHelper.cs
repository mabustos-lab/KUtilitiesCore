using KUtilitiesCore.DataAccess.Helpers;
using KUtilitiesCore.Extensions;
using KUtilitiesCore.Logger;
using KUtilitiesCore.MVVM.ActionResult;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.DataAccess.DAL
{
    /// <summary>
    /// Facilita la ejecución de procedimientos almacenados.
    /// </summary>
    public class StoreProcedureHelper : IStoreProcedureHelper
    {
        #region Fields

        private readonly List<Func<DbDataReader, IEnumerable>> _dataReaderResultMappers;
        private readonly List<Func<int, DataTable[], IEnumerable>> _dataTableResultMappers;
        private readonly List<IEnumerable> _results;

        private Action<IStoreProcedureHelper> _beforeExecuteAction;
        private CancellationToken _cancellationToken;
        private int _commandTimeout = -1;
        private bool _includeDebugParams;
        private ILoggerService<StoreProcedureHelper> _logger;
        private DataTable[] _resultDataTables;

        #endregion Fields

        #region Constructors

        private StoreProcedureHelper(string storeProcedureName)
        {
            StoreProcedureName = storeProcedureName;
            _dataReaderResultMappers = new List<Func<DbDataReader, IEnumerable>>();
            _results = new List<IEnumerable>();
            _dataTableResultMappers = new List<Func<int, DataTable[], IEnumerable>>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Colección de los parametros establecidos para la ejecución del Procedimeinto Almacenado
        /// </summary>
        public IDaoParameterCollection Parameters { get; private set; }

        /// <inheritdoc/>
        public IActionResult SPExecutionResult { get; private set; }

        /// <inheritdoc/>
        public string StoreProcedureName { get; private set; }

        #endregion Properties

        #region Methods

        public static IStoreProcedureHelper Create(string storeProcedureName)
        {
            return new StoreProcedureHelper(storeProcedureName);
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper BeforeExecAction(Action<IStoreProcedureHelper> action)
        {
            _beforeExecuteAction = action;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper ExecuteScalar<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer = null) where TContext : IDaoContext
        {
            ExecuteCore(context, withParamsInitializer, command =>
            {
                object result = command.ExecuteScalar();
                SPExecutionResult = result != null ? ActionResult<object>.CreateSuccess(result) : ActionResult.SuccessResult;
            });
            return this;
        }

        /// <inheritdoc/>
        public async Task<IStoreProcedureHelper> ExecuteScalarAsync<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer = null) where TContext : IDaoContext
        {
            await ExecuteCoreAsync(context, withParamsInitializer, async command =>
            {
                object result = await command.ExecuteScalarAsync(_cancellationToken).ConfigureAwait(false);
                SPExecutionResult = result != null ? ActionResult<object>.CreateSuccess(result) : ActionResult.SuccessResult;
            });
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper ExecuteStoreProcedure<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer = null) where TContext : IDaoContext
        {
            ExecuteCore(context, withParamsInitializer, command =>
            {
                using (var reader = command.ExecuteReader())
                {
                    foreach (var resultSetMapper in _dataReaderResultMappers)
                    {
                        _results.Add(resultSetMapper(reader));
                        reader.NextResult();
                    }
                }
                SPExecutionResult = ActionResult.SuccessResult;
            });
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper GetDataSet<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer = null) where TContext : IDaoContext
        {
            ExecuteCore(context, withParamsInitializer, command =>
            {
                using var adapter = context.CreateAdapter(command);
                using var dataSet = new DataSet();

                adapter.Fill(dataSet);

                if (dataSet.Tables.Count > 0)
                {
                    _resultDataTables = new DataTable[dataSet.Tables.Count];
                    dataSet.Tables.CopyTo(_resultDataTables, 0);
                }

                int idx = 0;
                foreach (var resultSetMapper in _dataTableResultMappers)
                {
                    _results.Add(resultSetMapper(idx, _resultDataTables));
                    idx++;
                }

                SPExecutionResult = ActionResult.SuccessResult;
            });
            return this;
        }

        /// <inheritdoc/>
        public TValue GetParamValue<TValue>(string paramName)
        {
            if (Parameters != null && Parameters.Contains(paramName))
            {
                var paramValue = Parameters[paramName].Value;
                if (paramValue != null && paramValue != DBNull.Value)
                {
                    return (TValue)paramValue;
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public IEnumerable<DataTable> GetResultDataTable()
        {
            return _resultDataTables ?? Enumerable.Empty<DataTable>();
        }

        /// <inheritdoc/>
        public IEnumerable<TResult> GetResultSet<TResult>(int indexResult = 0)
        {
            if (_results != null && _results.Count > indexResult)
            {
                return _results[indexResult].OfType<TResult>();
            }
            return Enumerable.Empty<TResult>();
        }

        /// <inheritdoc/>
        public bool HasResultSets() => _dataReaderResultMappers.Count > 0 && _results?.Count > 0;

        /// <inheritdoc/>
        public IStoreProcedureHelper SetCancelToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper SetCommandTimeout(int timeout)
        {
            if (timeout < 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "El valor debe ser mayor o igual a CERO.");
            _commandTimeout = timeout;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper SetLoger(ILoggerServiceFactory loggerServiceFactory, bool includeParams = false)
        {
            _logger = loggerServiceFactory.GetLogger<StoreProcedureHelper>();
            _includeDebugParams = includeParams;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper WithDTResult<TResult>() where TResult : class, new()
        {
            _dataTableResultMappers.Add((idx, tables) =>
                tables != null && tables.Length > idx
                    ? tables[idx].MapTo<TResult>().ToList()
                    : Enumerable.Empty<TResult>()
            );
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper WithResult<TResult>() where TResult : class, new()
        {
            _dataReaderResultMappers.Add(reader =>
            {
                if (reader == null || reader.IsClosed || reader.FieldCount == 0)
                    return Enumerable.Empty<TResult>();
                try
                {
                    return reader.Translate<TResult>().ToList();
                }
                catch (Exception ex)
                {
                    ex.Data.Add("TypeResult", typeof(TResult).FullName);
                    _logger?.LogError(ex, "Ocurrio un problema al convertir los resultados a '{Name}'.", typeof(TResult).Name);
                    return Enumerable.Empty<TResult>();
                }
            });
            return this;
        }

        private DbCommand CreateCommand(DbConnection dbCnn, Action<IDaoParameterCollection> withParamsInitializer)
        {
            DbCommand cmd = dbCnn.CreateCommand();
            cmd.CommandText = StoreProcedureName;
            cmd.CommandType = CommandType.StoredProcedure;

            if (_commandTimeout != -1)
                cmd.CommandTimeout = _commandTimeout;

            if (withParamsInitializer != null)
            {
                Parameters = new DaoParameterCollection(cmd.CreateParameter);
                withParamsInitializer(Parameters);
                if (Parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(Parameters.ToArray());
                }
            }

            return cmd;
        }

        /// <summary>
        /// Centraliza la lógica de ejecución síncrona.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context"></param>
        /// <param name="withParamsInitializer"></param>
        /// <param name="commandExecutor"></param>
        private void ExecuteCore<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer, Action<DbCommand> commandExecutor) where TContext : IDaoContext
        {
            DbCommand command = null;
            try
            {
                var connection = context.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (_logger != null && connection is SqlConnection sqlConnection)
                {
                    sqlConnection.InfoMessage += (s, e) => _logger.LogDebug($"[{StoreProcedureName}]: {e.Message}", CommonEventsID.DataAccess);
                }

                using (command = CreateCommand(connection, withParamsInitializer))
                {
                    _cancellationToken.Register(() => command.Cancel());
                    _beforeExecuteAction?.Invoke(this);

                    _logger?.LogDebug("Ejecutando: {StoreProcedureName}", StoreProcedureName);
                    commandExecutor(command);
                }
            }
            catch (OperationCanceledException ex)
            {
                SPExecutionResult = ActionResult.CancelResult;
                _logger?.LogWarning(ex, "La ejecución del comando fue cancelada.");
            }
            catch (Exception ex)
            {
                HandleException(ex, command);
            }
        }

        /// <summary>
        /// Centraliza la lógica de ejecución síncrona.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="context"></param>
        /// <param name="withParamsInitializer"></param>
        /// <param name="commandExecutor"></param>
        /// <returns></returns>
        private async Task ExecuteCoreAsync<TContext>(TContext context, Action<IDaoParameterCollection> withParamsInitializer, Func<DbCommand, Task> commandExecutor) where TContext : IDaoContext
        {
            DbCommand command = null;
            try
            {
                var connection = context.Connection;
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(_cancellationToken).ConfigureAwait(false);

                if (_logger != null && connection is SqlConnection sqlConnection)
                {
                    sqlConnection.InfoMessage += (s, e) => _logger.LogDebug($"[{StoreProcedureName}]: {e.Message}", CommonEventsID.DataAccess);
                }

                using (command = CreateCommand(connection, withParamsInitializer))
                {
                    _cancellationToken.Register(() => command.Cancel());
                    _beforeExecuteAction?.Invoke(this);

                    _logger?.LogDebug("Ejecutando (async): {StoreProcedureName}", StoreProcedureName);
                    await commandExecutor(command).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                SPExecutionResult = ActionResult.CancelResult;
                _logger?.LogWarning(ex, "La ejecución del comando fue cancelada.");
            }
            catch (Exception ex)
            {
                HandleException(ex, command);
            }
        }

        /// <summary>
        /// Centraliza la logica de maneho de excepciones
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="command"></param>
        private void HandleException(Exception ex, DbCommand command)
        {
            ex.Data.Add("StoreProcedureNameFail", StoreProcedureName);
            if (_includeDebugParams && command?.Parameters?.Count > 0)
            {
                foreach (DbParameter param in command.Parameters)
                {
                    ex.Data.Add(param.ParameterName, param.Value ?? DBNull.Value);
                }
            }
            SPExecutionResult = ActionResult.CreateFaultedResult($"Ocurrio un problema al ejecutar el procedimiento '{StoreProcedureName}'.", ex);
            _logger?.LogError(ex, "Ocurrio un problema al ejecutar el procedimiento '{StoreProcedureName}'.", StoreProcedureName);
        }

        #endregion Methods
    }
}