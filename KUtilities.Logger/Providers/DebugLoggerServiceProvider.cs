using KUtilitiesCore.Logger.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger.Providers
{
    /// <summary>
    /// Proveedor para crear instancias de <see cref="DebugWindowLogger{TCategoryName}"/>.
    /// </summary>
    /// <remarks>
    /// Inicializa una nueva instancia de la clase <see cref="DebugLoggerServiceProvider"/>.
    /// </remarks>
    /// <param name="options">Las opciones de configuración para el DebugWindowLogger.</param>
    /// <exception cref="ArgumentNullException">Si las opciones son nulas.</exception>
    public class DebugLoggerServiceProvider(LoggerOptions options) : ILoggerServiceProvider
    {
        private readonly LoggerOptions _options = options ?? throw new ArgumentNullException(nameof(options));
        /// <inheritdoc/>
        public string Name => "Debug"; // Nombre único para este proveedor

        /// <inheritdoc/>
        public ILoggerService<TCategoryName> CreateLogger<TCategoryName>()
        {
            return new DebugWindowLogger<TCategoryName>(_options);
        }
    }
}
