using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de <see cref="IAppLogger{TCategoryName}"/> que utiliza
    /// <see cref="ILogger{TCategoryName}"/> de Microsoft.Extensions.Logging.
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
        /// <param name="categoryName">El tipo de la clase que realiza el logging, usado para categorizar los mensajes.</param>
        /// <exception cref="ArgumentNullException">Se lanza si logger es null.</exception>
        protected LoggerServiceAbstract(ILoggerOptions? options= null)
        {
            Options = options ?? new LoggerOptions();
            CategoryName = typeof(TCategoryName).Name;
        }

        internal abstract void WriteLog(LogLevel logLevel, EventId eventId, Exception? exception, string message,params object[] args);
        /// <inheritdoc/>
        public void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args)
        {
            WriteLog(logLevel, eventId, exception, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, EventId eventId, string message, params object[] args)
        {
            WriteLog(logLevel, eventId, null, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, Exception exception, string message, params object[] args)
        {
            WriteLog(logLevel, 0, exception, message, args);
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, string message, params object[] args)
        {
            WriteLog(logLevel, 0, null, message, args);
        }

        // Implementaciones de los métodos de conveniencia

        /// <inheritdoc/>
        public void LogTrace(string message, params object[] args)
            => Log(LogLevel.Trace, message, args);

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, params object[] args)
            => Log(LogLevel.Trace, exception, message, args);

        /// <inheritdoc/>
        public void LogDebug(string message, params object[] args)
            => Log(LogLevel.Debug, message, args);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, params object[] args)
            => Log(LogLevel.Debug, exception, message, args);

        /// <inheritdoc/>
        public void LogInformation(string message, params object[] args)
             => Log(LogLevel.Information, message, args);

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, params object[] args)
             => Log(LogLevel.Information, exception, message, args);

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] args)
            => Log(LogLevel.Warning, message, args);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, params object[] args)
             => Log(LogLevel.Warning, exception, message, args);

        /// <inheritdoc/>
        public void LogError(string message, params object[] args)
            => Log(LogLevel.Error, message, args);

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, params object[] args)
            => Log(LogLevel.Error, exception, message, args);
        /// <inheritdoc/>
        public void LogCritical(string message, params object[] args)
            => Log(LogLevel.Critical, message, args);

        /// <inheritdoc/>
        public void LogCritical(Exception exception, string message, params object[] args)
            => Log(LogLevel.Critical, exception, message, args);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        => logLevel >= Options.MinimumLogLevel;
    }
}
