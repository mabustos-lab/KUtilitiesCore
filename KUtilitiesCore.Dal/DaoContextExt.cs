using System.Data;
using KUtilitiesCore.Dal.SQLLog;
using System.Data.Common;



#if NET8_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Funcionalidades extras al DaoContext
    /// </summary>
    public static class DaoContextExt
    {
        #region Methods
        /// <summary>
        /// Configura el logging de mensajes SQL con opciones avanzadas para SqlConnection.
        /// </summary>
        public static IDisposable EnableSqlLogging<TDbConnection>(this TDbConnection connection,
            Action<SqlLogEntry> logAction,
            SqlLoggingOptions? options = null)
            where TDbConnection : DbConnection
        {
            if (connection is SqlConnection sqlConn)
            {
                return SqlConnectionLogger.Attach(sqlConn, logAction, options);
            }
           
            else
            {
                logAction?.Invoke(new SqlLogEntry
                {
                    Message = $"La conexión {connection.GetType().Name} no soporta logging de InfoMessage.",
                    Timestamp = DateTime.UtcNow,
                    LogLevel = SqlLogLevel.Warning
                });
                return NullDisposable.Instance;
            }
        }

        /// <summary>
        /// Obtiene el nombre del servidor publicado asociado al contexto de datos.
        /// </summary>
        /// <param name="context">Contexto de ejecución SQL cuya conexión se usará.</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>El nombre del servidor publicado como una cadena.</returns>
        public static async Task<string> GetPublishedServerNameAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return await context.ScalarAsync<string>("SELECT PUBLISHINGSERVERNAME() as Servername")
                .ConfigureAwait(false)
                ?? throw new InvalidOperationException("El servidor no devolvió un nombre de servidor publicado.");
        }

        /// <summary>
        /// Obtiene la fecha y hora actual del servidor.
        /// </summary>
        /// <param name="context">Contexto de ejecución SQL cuya conexión se usará.</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>Un <see cref="DateTime"/> que representa la fecha y hora actual del servidor.</returns>
        public static async Task<DateTime> GetServerDateTimeAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return await context.ScalarAsync<DateTime>("SELECT SYSDATETIME() AS FechaHoraActual")
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Ejecuta sp_set_session_context para la conexión abierta, para establecer un contexto de
        /// sesión en la base de datos. Pensado para configurarse desde los hooks de apertura
        /// (<see cref="ISqlExecutorContext.OnConnectionOpened"/>/<see cref="ISqlExecutorContext.OnConnectionOpenedAsync"/>)
        /// y habilitar escenarios de RLS o auditoría por usuario.
        /// </summary>
        /// <param name="context">Contexto de ejecución SQL cuya conexión se usará.</param>
        /// <param name="sessionValue">Objeto que representa el nombre de la clave (Key) y su valor (Value)</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <remarks>
        /// Los valores de sesion solo pueden ser usados en el mismo contexto de conexión, al cerrar la conexión se pierden los valores.
        /// Invocado desde los hooks de apertura, <see cref="ISqlExecutorContext.Connection"/> ya está
        /// publicada y es seguro de leer (la llamada re-ingresa por el campo publicado, no por el factory lazy).
        /// </remarks>
        /// <example>
        /// Configuración típica para RLS/auditoría al abrir la conexión:
        /// <code>
        /// var context = new DaoContext(builder);
        /// context.OnConnectionOpenedAsync = async ctx =>
        ///     await ctx.SetSessionContextAsync(new KeyValuePair&lt;string, object&gt;>("Usuario", "jPerez"))
        ///             .ConfigureAwait(false);
        /// </code>
        /// </example>
        public static async Task SetSessionContextAsync<TDAO>(this TDAO context,
            KeyValuePair<string,object> sessionValue, CancellationToken cancellationToken = default) 
            where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            IDaoParameterCollection dbParameterCollection = context.CreateParameterCollection();
            dbParameterCollection.Add("key", sessionValue.Key);
            dbParameterCollection.Add("value", sessionValue.Value);
            await context.ExecuteNonQueryAsync("EXEC sp_set_session_context @key = @key, @value = @value", dbParameterCollection)
                .ConfigureAwait(false);
        }

        #endregion Methods
    }
}