using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de ILoggerService que no realiza ninguna acción (Null Object Pattern).
    /// Es útil para desactivar el logging sin romper el código cliente. Esta clase es sellada y usa un singleton.
    /// </summary>
    public sealed class NullLoggerService<TCategoryName> : ILoggerService<TCategoryName>
    {
        /// <summary>
        /// Instancia singleton para evitar creaciones innecesarias.
        /// </summary>
        public static NullLoggerService<TCategoryName> Instance { get; } = new NullLoggerService<TCategoryName>();

        private NullLoggerService() { }
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => false;
        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        /// <inheritdoc/>
        public void LogTrace(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogDebug(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogInformation(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogWarning(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogError(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogError(Exception exception, string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogCritical(string message, EventId? eventId, params object[] args) { }
        /// <inheritdoc/>
        public void LogCritical(Exception exception, string message, EventId? eventId, params object[] args) { }

        /// <summary>
        /// Clase interna para el scope que no hace nada.
        /// </summary>
        private sealed class NullScope : IDisposable
        {
            /// <summary>
            /// Instancia singleton para evitar creaciones innecesarias.
            /// </summary>
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }
    }
}
