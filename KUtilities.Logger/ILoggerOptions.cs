using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Define las opciones básicas para la configuración de un logger.
    /// </summary>
    public interface ILoggerOptions
    {
        /// <summary>
        /// Obtiene o establece el nivel mínimo de log que será registrado.
        /// </summary>
        LogLevel MinimumLogLevel { get; set; }
    }
}
