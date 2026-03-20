using KUtilitiesCore.Logger.Options;
using System;

namespace KUtilitiesCore.Logger.Providers
{
    /// <summary>
    /// Proveedor para crear instancias de <see cref="SqlLogger{TCategoryName}"/>.
    /// </summary>
    public class SqlLoggerServiceProvider : ILoggerServiceProvider
    {
        private readonly SqlLoggerOptions _options;

        /// <inheritdoc/>
        public string Name => nameof(SqlLoggerServiceProvider);

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SqlLoggerServiceProvider"/>.
        /// </summary>
        /// <param name="options">Opciones de configuración para SQL Logger.</param>
        public SqlLoggerServiceProvider(SqlLoggerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            if (string.IsNullOrEmpty(_options.ConnectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(options));
        }

        /// <inheritdoc/>
        public ILoggerService<TCategoryName> CreateLogger<TCategoryName>()
        {
            return new SqlLogger<TCategoryName>(_options);
        }
    }
}
