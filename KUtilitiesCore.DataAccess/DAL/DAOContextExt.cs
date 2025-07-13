using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.DAL
{
    public static class DAOContextExt
    {
        /// <summary>
        /// Obtiene el nombre del servidor publicado asociado al contexto de datos.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>El nombre del servidor publicado como una cadena.</returns>
        public static Task<string> GetPublishedServerNameAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : IDaoContext
        { return context.ScalarAsync<string>("SELECT PUBLISHINGSERVERNAME() as Servername"); }

        /// <summary>
        /// Obtiene la fecha y hora actual del servidor.
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        /// <returns>Un <see cref="DateTime"/> que representa la fecha y hora actual del servidor.</returns>
        public static Task<DateTime> GetServerDateTimeAsync<TDAO>(this TDAO context,
            CancellationToken cancellationToken = default) where TDAO : IDaoContext
        { return context.ScalarAsync<DateTime>("SELECT SYSDATETIME() AS FechaHoraActual"); }

        /// <summary>
        /// Ejecuta sp_set_session_context para la conexión abierta,
        /// para establecer un contexto de sesión en la base de datos.
        /// </summary>
        /// <param name="context">Conexión ADO.NET (DbConnection).</param>
        /// <param name="key">Clave del contexto (ej. "CurrentUserID").</param>
        /// <param name="value">Valor para establecer (puede ser GUID, int, string, etc.).</param>
        /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
        public static async Task SetSessionContextAsync<TDAO>(this TDAO context,
            string key,object value, CancellationToken cancellationToken = default) where TDAO : IDaoContext
        {
            if (context.Connection.State != ConnectionState.Open)
                await context.Connection.OpenAsync(cancellationToken);

            IDbParameterCollection dbParameterCollection = context.CreateParameterCollection();
            dbParameterCollection.Add(nameof(key), key);
            dbParameterCollection.Add(nameof(value), value);
            await context.ExecuteNonQueryAsync("EXEC sp_set_session_context @key = @key, @value = @value", dbParameterCollection);
        }
    }
}
