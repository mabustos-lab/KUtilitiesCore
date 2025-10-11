using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.Logger;
using KUtilitiesCore.MVVM.ActionResult;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.SPHelper
{
    /// <summary>
    /// Interface para ayudar con la configuración de ejecución de procedimientos almacenados.
    /// </summary>
    public interface IStoreProcedureHelper
    {

        /// <summary>
        /// Proporciona un contexto para las operaciones de acceso a datos.
        /// </summary>
        IDaoContext Context { get; }

        /// <summary>
        /// Colección de parámetros de base de datos con funcionalidad de solo lectura y
        /// manipulación avanzada.
        /// </summary>
        IDaoParameterCollection Parameters { get; }

        /// <summary>
        /// Obtiene el nombre del procedimiento almacenado.
        /// </summary>
        string StoreProcedureName { get; }

        /// <summary>
        /// Agrega un parámetro basado en una propiedad.
        /// </summary>
        IStoreProcedureHelper AddParameter<TSource, TValue>(
            TSource sourceObj,
            Expression<Func<TSource, TValue>> propertyExpression,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro con valor inicial y configuraciones.
        /// </summary>
        IStoreProcedureHelper AddParameter<TType>(
            string parameterName,
            TType value,
            int size,
            byte scale,
            byte precision,
            ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro con valor inicial.
        /// </summary>
        IStoreProcedureHelper AddParameter<TType>(string parameterName, TType value, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Agrega un parámetro sin valor inicial.
        /// </summary>
        IStoreProcedureHelper AddParameter<TType>(string parameterName, ParameterDirection direction = ParameterDirection.Input);

        /// <summary>
        /// Define una acción a ejecutar antes de la ejecución del procedimiento almacenado.
        /// </summary>
        /// <param name="action">Acción a ejecutar.</param>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper BeforeExecAction(Action<IStoreProcedureHelper> action);

        /// <summary>
        /// Ejecuta procedimeitos que modifican valores como (INSERT/UPDATE/DELETE)
        /// </summary>
        int ExecuteNonQuery();

        /// <summary>
        /// Ejecuta procedimeitos que modifican valores de manera asincrona como (INSERT/UPDATE/DELETE)
        /// </summary>
        Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta un procedimiento y devuelve uno o varios resultados.
        /// </summary>
        IReaderResultSet ExecuteReader();

        /// <summary>
        /// Ejecuta un procedimiento almacenado y devuelve uno o varios resultados de manera asincrona.
        /// </summary>
        Task<IReaderResultSet> ExecuteReaderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta un procedimiento y devuelve un vlor único.
        /// </summary>
        /// <returns>Regresa un valor con el tipo especificado TResult</returns>
        TResult ExecuteScalar<TResult>();

        /// <summary>
        /// Ejecuta un procedimiento y devuelve un vlor único de manera asincrona.
        /// </summary>
        /// <returns>Regresa un valor con el tipo especificado TResult</returns>
        Task<TResult> ExecuteScalarAsync<TResult>(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el valor de un parámetro.
        /// </summary>
        /// <typeparam name="TValue">Tipo del valor del parámetro.</typeparam>
        /// <param name="paramName">Nombre del parámetro.</param>
        /// <returns>Valor del parámetro.</returns>
        TValue GetParamValue<TValue>(string paramName);

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
        /// Define el tipo de resultado transformandolo directamente del <see cref="DbDataReader"/>.
        /// </summary>
        /// <typeparam name="TResult">Tipo del resultado.</typeparam>
        /// <returns>Instancia de <see cref="IStoreProcedureHelper"/>.</returns>
        IStoreProcedureHelper WithResult<TResult>() where TResult : class, new();

    }
}