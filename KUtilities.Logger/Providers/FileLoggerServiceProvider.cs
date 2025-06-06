using KUtilitiesCore.Logger.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger.Providers
{
    /// <summary>
    /// Proveedor para crear instancias de <see cref="FileLogger{TCategoryName}"/>.
    /// </summary>
    public class FileLoggerServiceProvider : ILoggerServiceProvider,IDisposable
    {
        private readonly FileLoggerOptions _options;
        private bool disposedValue;
        /// <inheritdoc/>
        public string Name => nameof(FileLoggerServiceProvider); // Nombre único para este proveedor

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FileLoggerServiceProvider"/>.
        /// </summary>
        /// <param name="options">Las opciones de configuración para el FileLogger.</param>
        /// <exception cref="ArgumentNullException">Si las opciones son nulas.</exception>
        public FileLoggerServiceProvider(FileLoggerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public ILoggerService<TCategoryName> CreateLogger<TCategoryName>()
        {
            // FileLogger es IDisposable, la fábrica o el contenedor DI
            // deberían manejar su ciclo de vida si es necesario un seguimiento global.
            // Por ahora, el proveedor simplemente lo crea.
            return new FileLogger<TCategoryName>(_options);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        // ~FileLoggerServiceProvider()
        // {
        //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
