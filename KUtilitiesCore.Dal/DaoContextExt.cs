using System.Data;

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
        /// Si la conexión es SqlConnection, suscribe un handler al evento InfoMessage
        /// para capturar mensajes de debug (PRINT, RAISERROR, warnings).
        /// </summary>
        public static void EnableSqlLogging<TDAO>(this TDAO context, Action<string> logAction)
            where TDAO : ISqlExecutorContext
        {
            if ( context.Connection is SqlConnection sqlConn)
            {
                // Limpia suscripciones previas para evitar duplicados
                sqlConn.InfoMessage -= OnInfoMessage;
                sqlConn.InfoMessage += OnInfoMessage;
                void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
                {
                    foreach (SqlError err in e.Errors)
                    {
                        logAction?.Invoke($"[SQL] {err.Message} (Número: {err.Number}, Severidad: {err.Class})");
                    }
                }
            }
            else
            {
                // Para otros proveedores no hay InfoMessage
                logAction?.Invoke("La conexión no es SqlConnection, no se puede habilitar logging de InfoMessage.");
            }
        }

        /// <summary>
        /// Obtiene el nombre del servidor publicado asociado al contexto de datos.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>El nombre del servidor publicado como una cadena.</returns>
        public static async Task<string> GetPublishedServerNameAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken);
            return await context.ScalarAsync<string>("SELECT PUBLISHINGSERVERNAME() as Servername");
        }

        /// <summary>
        /// Obtiene la fecha y hora actual del servidor.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>Un <see cref="DateTime"/> que representa la fecha y hora actual del servidor.</returns>
        public static async Task<DateTime> GetServerDateTimeAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken);
            return await context.ScalarAsync<DateTime>("SELECT SYSDATETIME() AS FechaHoraActual");
        }

        /// <summary>
        /// Ejecuta sp_set_session_context para la conexión abierta, para establecer un contexto de
        /// sesión en la base de datos.
        /// </summary>
        /// <param name="context">Conexión ADO.NET (DbConnection).</param>
        /// <param name="sessionValue">Objeto que representa el nombre de la clave (Key) y su valor (Value)</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <remarks>
        /// Los valores de sesion solo pueden ser unsados en el mismo contexto de conexión, al cerrar la conexión se pierden los valores.
        /// </remarks>
        public static async Task SetSessionContextAsync<TDAO>(this TDAO context,
            KeyValuePair<string,object> sessionValue, CancellationToken cancellationToken = default) 
            where TDAO : ISqlExecutorContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken);

            IDaoParameterCollection dbParameterCollection = context.CreateParameterCollection();
            dbParameterCollection.Add("key", sessionValue.Key);
            dbParameterCollection.Add("value", sessionValue.Value);
            await context.ExecuteNonQueryAsync("EXEC sp_set_session_context @key = @key, @value = @value", dbParameterCollection);
        }

        #endregion Methods
    }
}