
using KUtilitiesCore.Logger;
using KUtilitiesCore.MVVM.ActionResult;
using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Interface para ayudar con la ejecución de procedimientos almacenados.
    /// </summary>
    public interface IStoreProcedureHelper
    {
        #region Properties

        /// <summary>
        /// Obtiene el resultado de la ejecución del procedimiento almacenado.
        /// </summary>
        IActionResult SPExecutionResult { get; }

        /// <summary>
        /// Obtiene el nombre del procedimiento almacenado.
        /// </summary>
        string StoreProcedureName { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Define una acción a ejecutar antes de la ejecución del procedimiento almacenado.
        /// </summary>
        /// <param name="action">Acción a ejecutar.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper BeforeExecAction(Action<IStoreProcedureHelper> action);

        ///// <summary>
        ///// Ejecuta un procedimiento almacenado y devuelve un valor escalar.
        ///// </summary>
        ///// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        ///// <typeparam name="TResult">Tipo del resultado.</typeparam>
        ///// <param name="context">Contexto de base de datos.</param>
        ///// <param name="resultConverter">Función para convertir el resultado.</param>
        ///// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        ///// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        //IStoreProcedureHelper ExecuteScalar<TContext, TResult>(TContext context,
        //    Func<object, TResult> resultConverter,
        //    Action<IDaoParameterCollection> withParamsInitializer = null)
        //    where TContext : IDaoContext;

        /// <summary>
        /// Ejecuta un procedimiento almacenado sin esperar algun valor resultado.
        /// </summary>
        /// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper ExecuteScalar<TContext>(TContext context,
         Action<IDaoParameterCollection> withParamsInitializer = null)
         where TContext : IDaoContext;

        /// <summary>
        /// Ejecuta un procedimiento almacenado y devuelve un valor escalar de manera asincrona.
        /// </summary>
        /// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        Task<IStoreProcedureHelper> ExecuteScalarAsync<TContext>(TContext context,
         Action<IDaoParameterCollection> withParamsInitializer = null)
         where TContext : IDaoContext;

        ///// <summary>
        ///// Ejecuta un procedimiento almacenado y devuelve un valor escalar de manera asincrona.
        ///// </summary>
        ///// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        ///// <typeparam name="TResult">Tipo del resultado.</typeparam>
        ///// <param name="context">Contexto de base de datos.</param>
        ///// <param name="resultConverter">Función para convertir el resultado.</param>
        ///// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        ///// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        //Task<IStoreProcedureHelper> ExecuteScalarAsync<TContext, TResult>(TContext context,
        //    Func<object, TResult> resultConverter,
        //    Action<IDaoParameterCollection> withParamsInitializer = null)
        //    where TContext : IDaoContext;

        /// <summary>
        /// Ejecuta un procedimiento almacenado.
        /// </summary>
        /// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper ExecuteStoreProcedure<TContext>(TContext context,
            Action<IDaoParameterCollection> withParamsInitializer = null)
            where TContext : IDaoContext;

        /// <summary>
        /// Ejecuta un procedimiento almacenado y almacena los resultados en un conjunto de datos en DataSet.
        /// </summary>
        /// <typeparam name="TContext">Tipo del contexto de base de datos.</typeparam>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="withParamsInitializer">Inicializador de parámetros.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper GetDataSet<TContext>(TContext context,
            Action<IDaoParameterCollection> withParamsInitializer = null)
            where TContext : IDaoContext;

        /// <summary>
        /// Obtiene el valor de un parámetro.
        /// </summary>
        /// <typeparam name="TValue">Tipo del valor del parámetro.</typeparam>
        /// <param name="paramName">Nombre del parámetro.</param>
        /// <returns>Valor del parámetro.</returns>
        TValue GetParamValue<TValue>(string paramName);

        /// <summary>
        /// Obtiene los resultados en DataTables del DataSet obtenido por el procedimeinto.
        /// </summary>
        /// <returns>Array de <see cref="DataTable"/> con los resultados.</returns>
        IEnumerable<DataTable> GetResultDataTable();

        /// <summary>
        /// Obtiene un conjunto de resultados fuertemente tipado.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <param name="indexResult">Índice del conjunto de resultados.</param>
        /// <returns>Enumerable de resultados.</returns>
        IEnumerable<TResult> GetResultSet<TResult>(int indexResult = 0);

        /// <summary>
        /// Indica si hay conjuntos de resultados disponibles.
        /// </summary>
        /// <returns>True si hay conjuntos de resultados, de lo contrario false.</returns>
        bool HasResultSets();

        /// <summary>
        /// Establece el token de cancelación.
        /// </summary>
        /// <param name="cts">Token de cancelación.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper SetCancelToken(CancellationToken cts);

        /// <summary>
        /// Establece el tiempo de espera del comando.
        /// </summary>
        /// <param name="timeout">Tiempo de espera en segundos.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper SetCommandTimeout(int timeout);

        /// <summary>
        /// establece el servicio de Log para depurar los procedimeintos almacenados.
        /// </summary>
        /// <param name="loggerServiceFactory"></param>
        /// ///
        /// <param name="includeParams">Indica se se expone la información sensible de los parametros</param>
        /// <returns></returns>
        IStoreProcedureHelper SetLoger(ILoggerServiceFactory loggerServiceFactory, bool includeParams = false);

        /// <summary>
        /// Define el tipo de resultado transformandolo de un DataTable.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper WithDTResult<TResult>() where TResult : class, new();

        /// <summary>
        /// Define el tipo de resultado transformandolo directamente del <see cref="DbDataReader"/>.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper WithResult<TResult>() where TResult : class, new();

        #endregion Methods
    }
}