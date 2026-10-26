using System.Data;
using System.Data.Common;

namespace KUtilitiesCore.Dal
{
    /// <summary>
    /// Interfaz para operaciones básicas de ejecución SQL sobre un contexto de acceso a datos.
    /// Permite ejecutar comandos SQL, consultas escalares y operaciones asincrónicas, así como la
    /// gestión de parámetros y transacciones.
    /// </summary>
    public interface ISqlExecutorContext : IDalContext
    {

        /// <summary>
        /// Obtiene la conexión de base de datos asociada al contexto.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Obtiene o establece el tiempo de espera (en segundos) antes de terminar el intento de
        /// ejecutar el comando y generar un error.
        /// </summary>
        int ConnectionTimeout { get; set; }

        /// <summary>
        /// Delegado que se invoca inmediatamente después de abrir la conexión (tras el logging
        /// SQL y antes del hook asíncrono <see cref="OnConnectionOpenedAsync"/>). Permite ejecutar
        /// lógica personalizada (ej. sp_set_session_context). Puede ser null si no se configura.
        /// </summary>
        /// <remarks>
        /// La conexión ya está publicada cuando el hook se invoca, por lo que <see cref="Connection"/>
        /// es seguro de leer desde el interior del handler (sin error de re-entrada del factory lazy).
        /// El hook se dispara exactamente una vez por conexión lógica: las sondas internas como
        /// <see cref="IDalContext.DatabaseExists"/> no lo invocan ni materializan la conexión lógica,
        /// y los accesos concurrentes esperan a que el hook complete y comparten la misma instancia.
        /// Si el handler lanza, la conexión se dispone y el contexto queda envenenado de forma
        /// permanente: todo acceso posterior re-lanza la excepción cacheada.
        /// </remarks>
        /// <example>
        /// Establecer el contexto de sesión para RLS/auditoría al abrir la conexión:
        /// <code>
        /// var context = new DaoContext(builder);
        /// context.OnConnectionOpened = ctx =>
        ///     ctx.SetSessionContextAsync(new KeyValuePair&lt;string, object&gt;>("Usuario", "jPerez"))
        ///        .GetAwaiter().GetResult();
        /// // La primera operación materializa la conexión y dispara el hook una única vez;
        /// // las operaciones posteriores reutilizan la misma conexión con el contexto de sesión.
        /// var nombre = context.Scalar&lt;string&gt;("SELECT SESSION_CONTEXT(N'Usuario')");
        /// </code>
        /// </example>
        Action<ISqlExecutorContext>? OnConnectionOpened { get; set; }

        /// <summary>
        /// Delegado asíncrono que se invoca después del hook síncrono <see cref="OnConnectionOpened"/>
        /// cuando la conexión se abre por primera vez. Permite ejecutar lógica asincrónica (ej.
        /// <c>SetSessionContextAsync</c> para RLS o auditoría) sin bloquear un hilo del pool.
        /// Puede ser null si no se configura.
        /// </summary>
        /// <remarks>
        /// La materialización de la conexión bloquea hasta que el delegado completa
        /// (invocación sync-over-async). Si el handler captura y continúa en un
        /// <c>SynchronizationContext</c> con afinidad de hilo (ej. UI de WinForms/WPF),
        /// se puede producir un interbloqueo: use <c>ConfigureAwait(false)</c> en el interior
        /// del handler. Si el handler lanza, la conexión se dispone y el contexto queda
        /// envenenado de forma permanente: todo acceso posterior a <see cref="Connection"/>
        /// re-lanza la excepción cacheada, por lo que el consumidor debe crear un contexto nuevo.
        /// </remarks>
        /// <example>
        /// Variante asíncrona del hook (preferible a la síncrona cuando el handler hace I/O):
        /// <code>
        /// var context = new DaoContext(builder);
        /// context.OnConnectionOpenedAsync = async ctx =>
        /// {
        ///     // ConfigureAwait(false) evita el interbloqueo del sync-over-async
        ///     // si el contexto se materializa sobre un hilo con SynchronizationContext.
        ///     await ctx.SetSessionContextAsync(new KeyValuePair&lt;string, object&gt;>("Usuario", "jPerez"))
        ///             .ConfigureAwait(false);
        /// };
        /// </code>
        /// </example>
        Func<ISqlExecutorContext, Task>? OnConnectionOpenedAsync { get; set; }

        /// <summary>
        /// Crea una nueva colección de parámetros para comandos SQL.
        /// </summary>
        /// <returns>Instancia de <see cref="IDaoParameterCollection"/> para agregar parámetros.</returns>
        IDaoParameterCollection CreateParameterCollection();

        /// <summary>
        /// Ejecuta un comando SQL que no retorna resultados (por ejemplo, INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando (Texto, Procedimiento almacenado, etc.).</param>
        /// <param name="transaction">Transacción opcional en la que ejecutar el comando.</param>
        /// <returns>Número de filas afectadas.</returns>
        int ExecuteNonQuery(string sql, IDaoParameterCollection? parameters = null,
                           CommandType commandType = CommandType.Text, ITransaction? transaction = null);

        /// <summary>
        /// Ejecuta asincrónicamente un comando SQL que no retorna resultados.
        /// </summary>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Número de filas afectadas.</returns>
        Task<int> ExecuteNonQueryAsync(string sql, IDaoParameterCollection? parameters = null,
                                      CommandType commandType = CommandType.Text, ITransaction? transaction = null,
                                      CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una consulta SQL y retorna el primer valor de la primera fila del resultado.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato esperado como resultado.</typeparam>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <returns>
        /// Valor escalar obtenido de la consulta; puede ser <c>null</c> (o el valor por defecto)
        /// cuando la consulta no retorna filas o el valor es <see cref="DBNull"/>.
        /// </returns>
        TResult? Scalar<TResult>(string sql, IDaoParameterCollection? parameters = null,
            CommandType commandType = CommandType.Text, ITransaction? transaction = null);

        /// <summary>
        /// Ejecuta asincrónicamente una consulta SQL y retorna el primer valor de la primera fila
        /// del resultado.
        /// </summary>
        /// <typeparam name="TResult">Tipo de dato esperado como resultado.</typeparam>
        /// <param name="sql">Cadena SQL a ejecutar.</param>
        /// <param name="parameters">Colección de parámetros para el comando.</param>
        /// <param name="commandType">Tipo de comando.</param>
        /// <param name="transaction">Transacción opcional.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>
        /// Valor escalar obtenido de la consulta; puede ser <c>null</c> (o el valor por defecto)
        /// cuando la consulta no retorna filas o el valor es <see cref="DBNull"/>.
        /// </returns>
        Task<TResult?> ScalarAsync<TResult>(string sql, IDaoParameterCollection? parameters = null,
            CommandType commandType = CommandType.Text, ITransaction? transaction = null,
                                          CancellationToken cancellationToken = default);

    }
}