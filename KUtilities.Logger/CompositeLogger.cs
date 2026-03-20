using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de <see cref="ILoggerService{TCategoryName}"/> que distribuye los mensajes
    /// de log a múltiples servicios de logging (Composite Pattern).
    /// </summary>
    /// <typeparam name="TCategoryName">La categoría asociada al logger.</typeparam>
    public class CompositeLogger<TCategoryName> : ILoggerService<TCategoryName>
    {
        private readonly IEnumerable<ILoggerService<TCategoryName>> _loggers;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CompositeLogger{TCategoryName}"/>.
        /// </summary>
        /// <param name="loggers">La colección de loggers a los que se enviarán los mensajes.</param>
        public CompositeLogger(IEnumerable<ILoggerService<TCategoryName>> loggers)
        {
            _loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            var scopes = _loggers.Select(l => l.BeginScope(state)).Where(s => s != null).ToList();
            return new CompositeDisposable(scopes!);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => _loggers.Any(l => l.IsEnabled(logLevel));

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }

        /// <inheritdoc/>
        public void LogTrace(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogTrace(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogTrace(exception, message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogDebug(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogDebug(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogDebug(exception, message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogInformation(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogInformation(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogInformation(exception, message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogWarning(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogWarning(exception, message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogError(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogError(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogError(exception, message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogCritical(string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogCritical(message, eventId, args);
        }

        /// <inheritdoc/>
        public void LogCritical(Exception exception, string message, EventId? eventId, params object[] args)
        {
            foreach (var logger in _loggers) logger.LogCritical(exception, message, eventId, args);
        }
    }

    /// <summary>
    /// Agrupa múltiples <see cref="IDisposable"/> en una sola unidad.
    /// </summary>
    internal class CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}
