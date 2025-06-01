using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación básica de <see cref="ILoggerOptions"/> para configurar el nivel mínimo de log.
    /// </summary>
    public class LoggerOptions : ILoggerOptions
    {
        /// <inheritdoc/>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Trace;
    }
}
