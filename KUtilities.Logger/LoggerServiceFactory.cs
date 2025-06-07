using KUtilitiesCore.Logger.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Implementación de <see cref="ILoggerServiceFactory"/> que gestiona múltiples proveedores de logging,
    /// el almacenamiento en caché de instancias y su ciclo de vida.
    /// </summary>
    public class LoggerServiceFactory : ILoggerServiceFactory
    {
        private readonly Dictionary<string, ILoggerServiceProvider> _providers = new Dictionary<string, ILoggerServiceProvider>(StringComparer.OrdinalIgnoreCase);

        // Caché para almacenar instancias de logger y gestionarlas como singletons por categoría.
        // La clave externa es el nombre del proveedor, la clave interna es el tipo de la categoría.
        private readonly Dictionary<string, Dictionary<Type, object>> _loggerCache = new Dictionary<string, Dictionary<Type, object>>(StringComparer.OrdinalIgnoreCase);

        private bool _disposed = false;

        /// <inheritdoc/>
        public void AddProvider(ILoggerServiceProvider provider)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LoggerServiceFactory));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (string.IsNullOrWhiteSpace(provider.Name))
            {
                throw new ArgumentException("El nombre del proveedor no puede ser nulo o vacío.", nameof(provider));
            }
            if (_providers.ContainsKey(provider.Name))
            {
                throw new ArgumentException($"Ya existe un proveedor registrado con el nombre '{provider.Name}'.", nameof(provider));
            }
            _providers.Add(provider.Name, provider);
        }

        /// <inheritdoc/>
        public ILoggerService<TCategoryName> GetLogger<TCategoryName>(string providerName)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LoggerServiceFactory));
            }
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("El nombre del proveedor no puede ser nulo o vacío.", nameof(providerName));
            }
            if (providerName.Equals(nameof(NullLoggerServiceProvider), StringComparison.OrdinalIgnoreCase))
            {
                return NullLoggerService<TCategoryName>.Instance;
            }
            // Asegurarse de que el caché para este proveedor exista.
            if (!_loggerCache.TryGetValue(providerName, out var providerCache))
            {
                providerCache = new Dictionary<Type, object>();
                _loggerCache[providerName] = providerCache;
            }

            var categoryType = typeof(TCategoryName);
            // Buscar en el caché una instancia existente para esta categoría.
            if (providerCache.TryGetValue(categoryType, out var loggerInstance))
            {
                return (ILoggerService<TCategoryName>)loggerInstance;
            }

            // Si no está en el caché, crearla usando el proveedor.
            if (_providers.TryGetValue(providerName, out var provider))
            {
                var newLogger = provider.CreateLogger<TCategoryName>();
                // Guardar la nueva instancia en el caché.
                providerCache[categoryType] = newLogger;
                return newLogger;
            }

            throw new ArgumentException($"No se encontró ningún proveedor de logging con el nombre '{providerName}'.");
        }

        /// <inheritdoc/>
        public ILoggerService<TCategoryName> GetLogger<TCategoryName>()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LoggerServiceFactory));
            }
            if (_providers.Count == 0)
            {
                return NullLoggerService<TCategoryName>.Instance;
            }

            // Usar el primer proveedor registrado como el predeterminado.
            var defaultProviderName = _providers.Keys.First();
            // Llamar al método sobrecargado para aprovechar la lógica de caché.
            return GetLogger<TCategoryName>(defaultProviderName);
        }

        /// <summary>
        /// Libera los recursos utilizados por la fábrica, incluyendo la llamada a Dispose()
        /// en todas las instancias de logger en caché que implementen IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Recorrer todos los loggers en caché y liberarlos si son IDisposable.
                    foreach (var providerCache in _loggerCache.Values)
                    {
                        foreach (var logger in providerCache.Values)
                        {
                            if (logger is IDisposable disposableLogger)
                            {
                                disposableLogger.Dispose();
                            }
                        }
                    }
                    _loggerCache.Clear();
                    _providers.Clear();
                }
                _disposed = true;
            }
        }
    }
}
