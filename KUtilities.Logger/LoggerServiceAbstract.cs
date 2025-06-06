using KUtilitiesCore.Logger.Helpers;
using KUtilitiesCore.Logger.Options;
using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Clase base abstracta para servicios de logging personalizados.
    /// Implementa <see cref="ILoggerService{TCategoryName}"/> y utiliza las opciones de configuración
    /// definidas en <typeparamref name="TOptions"/> para controlar el comportamiento del log.
    /// Proporciona métodos de conveniencia para registrar mensajes en diferentes niveles de severidad,
    /// así como la gestión de scopes y el formateo de mensajes.
    /// </summary>
    /// <typeparam name="TCategoryName">
    /// Tipo de la clase que realiza el logging, utilizado para categorizar los mensajes.
    /// </typeparam>
    /// <typeparam name="TOptions">
    /// Tipo de las opciones de configuración del logger, que debe implementar <see cref="ILoggerOptions"/>.
    /// </typeparam>
    /// <remarks>
    /// Inicializa una nueva instancia de la clase <see cref="LoggerServiceAbstract{TCategoryName, TOptions}"/>.
    /// </remarks>
    /// <param name="options">Opciones de configuración del logger.</param>
    /// <exception cref="ArgumentNullException">Se lanza si <paramref name="options"/> es null.</exception>
    public abstract class LoggerServiceAbstract<TCategoryName, TOptions>(TOptions options)
        : ILoggerService<TCategoryName>
        where TOptions : ILoggerOptions
    {
        /// <summary>
        /// Nombre de la categoría asociada al logger, generalmente el nombre de la clase que lo utiliza.
        /// </summary>
        internal readonly string CategoryName = typeof(TCategoryName).Name;

        /// <summary>
        /// Opciones de configuración del logger.
        /// </summary>
        internal readonly TOptions LogOptions = options ?? throw new ArgumentNullException(nameof(options));

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new LoggerScope(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= LogOptions.MinimumLogLevel;

        /// <inheritdoc/>
        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);

            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            // Obtener el scope actual, si existe
            var scope = LoggerScope.Current;
            string scopeInfo = scope != null ? $"[Scope: {scope.State}]" : string.Empty;

            // Formato simple: [Nivel] [Categoría] [EventId] [Scope] Mensaje (Exception)
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{CategoryName}] [EventId: {eventId.Name}] {scopeInfo} {message}";
            var entry = new LogEntry(DateTime.Now, logLevel, eventId, exception, logMessage);
            WriteLog(entry);
        }

        /// <inheritdoc/>
        public void LogCritical(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Critical, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogCritical(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Critical, exception, eventId, message, args);

        /// <inheritdoc/>
        public void LogDebug(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Debug, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Debug, exception, eventId, message, args);

        /// <inheritdoc/>
        public void LogError(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Error, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Error, exception, eventId, message, args);

        /// <inheritdoc/>
        public void LogInformation(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Information, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Information, exception, eventId, message, args);

        /// <inheritdoc/>
        public void LogTrace(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Trace, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Trace, exception, eventId, message, args);

        /// <inheritdoc/>
        public void LogWarning(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Warning, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Warning, exception, eventId, message, args);

        /// <summary>
        /// Método que debe implementar la clase derivada para escribir la entrada de log en el destino deseado.
        /// </summary>
        /// <param name="entry">La entrada de log a escribir.</param>
        internal abstract void WriteLog(LogEntry entry);

        /// <summary>
        /// Método privado auxiliar para formatear y registrar mensajes de log usando los métodos de conveniencia.
        /// </summary>
        /// <param name="logLevel">Nivel de log.</param>
        /// <param name="exception">Excepción asociada, si existe.</param>
        /// <param name="eventId">Identificador del evento.</param>
        /// <param name="message">Mensaje de log.</param>
        /// <param name="args">Argumentos para el mensaje.</param>
        private void Log(LogLevel logLevel, Exception? exception, EventId? eventId, string message, params object[] args)
        {
            // Formatea el mensaje si hay argumentos
            string formattedMessage = $"[{CategoryName}]: " +
                ((args != null && args.Length > 0) ? string.Format(message, args) : message);

            // Usa el nombre de la categoría como EventId.Name y 0 como Id por defecto
            var eventId2 = eventId ?? new EventId(0, "NA");

            // Llama al método Log<TState> de la clase
            Log<string>(
                logLevel,
                eventId2,
                string.Empty,
                exception,
                (state, ex) => formattedMessage
            );
        }
    }
}