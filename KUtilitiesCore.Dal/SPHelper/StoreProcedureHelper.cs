using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.Logger;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace KUtilitiesCore.Dal.SPHelper
{
    /// <summary>
    /// Implementación de helper para ejecución de procedimientos almacenados con configuración fluida.
    /// </summary>
    class StoreProcedureHelper : IStoreProcedureHelper
    {
        #region Fields
        private readonly List<Action<IStoreProcedureHelper>> _beforeExecActions;
        private readonly IDataReaderConverter _dataReaderConverter;
        private bool _includeParamsInLog;
        private ILoggerService _logger;
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="StoreProcedureHelper"/>.
        /// </summary>
        /// <param name="context">Contexto de acceso a datos.</param>
        /// <param name="storeProcedureName">Nombre del procedimiento almacenado.</param>
        /// <exception cref="ArgumentNullException">
        /// Se lanza cuando <paramref name="context"/> o <paramref name="storeProcedureName"/> son nulos o vacíos.
        /// </exception>
        public StoreProcedureHelper(IDaoContext context, string storeProcedureName)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            StoreProcedureName = !string.IsNullOrWhiteSpace(storeProcedureName)
                ? storeProcedureName
                : throw new ArgumentException(
                    "El nombre del procedimiento almacenado no puede ser nulo o vacío.",
                    nameof(storeProcedureName));

            Parameters = context.CreateParameterCollection();
            _beforeExecActions = new List<Action<IStoreProcedureHelper>>();
            _dataReaderConverter = DataReaderConverter.Create().WithDefaultDataTable();
        }
        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public IDaoContext Context { get; }

        /// <inheritdoc/>
        public IDaoParameterCollection Parameters { get; }

        /// <inheritdoc/>
        public string StoreProcedureName { get; }
        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public IStoreProcedureHelper AddParameter<TSource, TValue>(
            TSource sourceObj,
            Expression<Func<TSource, TValue>> propertyExpression,
            ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.Add(sourceObj, propertyExpression, direction);
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper AddParameter<TType>(
            string parameterName,
            TType value,
            int size,
            byte scale,
            byte precision,
            ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.Add(parameterName, value, size, scale, precision, direction);
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper AddParameter<TType>(
            string parameterName,
            TType value,
            ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.Add(parameterName, value, direction);
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper AddParameter<TType>(
            string parameterName,
            ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.Add<TType>(parameterName, direction);
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper BeforeExecAction(Action<IStoreProcedureHelper> action)
        {
            if(action != null)
            {
                _beforeExecActions.Add(action);
            }
            return this;
        }

        /// <inheritdoc/>
        public int ExecuteNonQuery()
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteNonQuery");

            try
            {
                var result = Context.ExecuteNonQuery(StoreProcedureName, Parameters, CommandType.StoredProcedure);
                
                LogSuccess($"Filas afectadas: {result}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteNonQueryAsync");

            try
            {
                var result = await Context.ExecuteNonQueryAsync(
                    StoreProcedureName,
                    Parameters,
                    CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);

                LogSuccess($"Filas afectadas: {result}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public IReaderResultSet ExecuteReader()
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteReader");

            try
            {
                var result = Context.ExecuteReader(
                    StoreProcedureName,
                    _dataReaderConverter,
                    Parameters,
                    CommandType.StoredProcedure);

                LogSuccess($"Conjuntos de resultados: {result.ResultSetCount}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IReaderResultSet> ExecuteReaderAsync(CancellationToken cancellationToken = default)
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteReaderAsync");

            try
            {
                var result = await Context.ExecuteReaderAsync(
                    StoreProcedureName,
                    _dataReaderConverter,
                    Parameters,
                    CommandType.StoredProcedure,
                    cancellationToken);

                LogSuccess($"Conjuntos de resultados: {result.ResultSetCount}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public TResult ExecuteScalar<TResult>()
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteScalar");

            try
            {
                var result = Context.Scalar<TResult>(StoreProcedureName, Parameters);

                LogSuccess($"Resultado escalar: {result}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TResult> ExecuteScalarAsync<TResult>(CancellationToken cancellationToken = default)
        {
            ExecuteBeforeActions();
            LogExecution("ExecuteScalarAsync");

            try
            {
                var result = await Context.ScalarAsync<TResult>(StoreProcedureName, Parameters, cancellationToken);

                LogSuccess($"Resultado escalar: {result}");
                return result;
            } catch(Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public TValue GetParamValue<TValue>(string paramName) { return Parameters.GetParamValue<TValue>(paramName); }

        /// <inheritdoc/>
        public IStoreProcedureHelper SetCommandTimeout(int timeout)
        {
            Context.ConnectionTimeout = timeout;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper SetLoger(ILoggerServiceFactory loggerServiceFactory, bool includeParams = false)
        {
            _logger = loggerServiceFactory.GetLogger<IStoreProcedureHelper>() ??
                throw new ArgumentNullException(nameof(loggerServiceFactory));
            _includeParamsInLog = includeParams;
            return this;
        }

        /// <inheritdoc/>
        public IStoreProcedureHelper WithResult<TResult>() where TResult : class, new()
        {
            _dataReaderConverter.WithResult<TResult>();
            return this;
        }

        /// <summary>
        /// Ejecuta todas las acciones configuradas para ejecutarse antes de la operación principal.
        /// </summary>
        private void ExecuteBeforeActions()
        {
            foreach(var action in _beforeExecActions)
            {
                action(this);
            }
        }

        /// <summary>
        /// Registra un error durante la ejecución.
        /// </summary>
        /// <param name="exception">Excepción ocurrida.</param>
        private void LogError(Exception exception)
        { _logger?.LogError(exception, "Error ejecutando SP: {StoreProcedureName}", StoreProcedureName); }

        /// <summary>
        /// Registra información sobre la ejecución del procedimiento almacenado.
        /// </summary>
        /// <param name="operation">Nombre de la operación que se está ejecutando.</param>
        private void LogExecution(string operation)
        {
            _logger?.LogDebug($"Ejecutando {operation} para SP: {StoreProcedureName}");

            if(_includeParamsInLog && _logger != null)
            {
                foreach(var param in Parameters)
                {
                    _logger.LogDebug($"Parámetro: {param.ParameterName} = {param.Value}, Dirección: {param.Direction}");
                }
            }
        }

        /// <summary>
        /// Registra una ejecución exitosa.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        private void LogSuccess(string message)
        { _logger?.LogInformation($"SP {StoreProcedureName} ejecutado exitosamente. {message}"); }
        #endregion Methods
    }
}