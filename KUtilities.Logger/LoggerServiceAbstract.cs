using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de <see cref="IAppLogger{TCategoryName}"/> que utiliza <see
    /// cref="ILogger{TCategoryName}"/> de Microsoft.Extensions.Logging.
    /// </summary>
    /// <typeparam name="TCategoryName">El tipo de la clase que realiza el logging.</typeparam>
    public abstract class LoggerServiceAbstract<TCategoryName> : ILoggerService<TCategoryName>
    {

        internal readonly string CategoryName;
        internal readonly ILoggerOptions Options;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AppLogger{TCategoryName}"/>.
        /// </summary>
        /// <param name="logger">La instancia de ILogger proporcionada por el framework de logging.</param>
        /// <param name="categoryName">
        /// El tipo de la clase que realiza el logging, usado para categorizar los mensajes.
        /// </param>
        /// <exception cref="ArgumentNullException">Se lanza si logger es null.</exception>
        protected LoggerServiceAbstract(ILoggerOptions? options = null)
        {
            Options = options ?? new LoggerOptions();
            CategoryName = typeof(TCategoryName).Name;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new LoggerScope(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        => logLevel >= Options.MinimumLogLevel;

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

        // Implementaciones de los métodos de conveniencia
        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Trace, exception, eventId, message, args);

        //internal abstract void WriteLog(LogLevel logLevel, EventId eventId, Exception? exception, string message, params object[] args);
        /// <inheritdoc/>
        public void LogWarning(string message, EventId? eventId, params object[] args)
            => Log(LogLevel.Warning, null, eventId, message, args);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, EventId? eventId, params object[] args)
             => Log(LogLevel.Warning, exception, eventId, message, args);

        internal abstract void WriteLog(LogEntry entry);

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