using KUtilitiesCore.Logger.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Define un contrato para una fábrica de servicios de logging.
    /// La fábrica utiliza proveedores (<see cref="ILoggerServiceProvider"/>) para crear
    /// instancias de <see cref="ILoggerService{TCategoryName}"/>.
    /// </summary>
    public interface ILoggerServiceFactory : IDisposable
    {
        /// <summary>
        /// Agrega un proveedor de servicios de logging a la fábrica.
        /// </summary>
        /// <param name="provider">El proveedor de servicios de logging a agregar.</param>
        /// <exception cref="ArgumentNullException">Si el proveedor es nulo.</exception>
        /// <exception cref="ArgumentException">Si ya existe un proveedor con el mismo nombre.</exception>
        void AddProvider(ILoggerServiceProvider provider);

        /// <summary>
        /// Obtiene una instancia de <see cref="ILoggerService{TCategoryName}"/> utilizando un proveedor específico.
        /// </summary>
        /// <typeparam name="TCategoryName">El tipo usado para la categoría del logger.</typeparam>
        /// <param name="providerName">El nombre del proveedor a utilizar.</param>
        /// <returns>Una instancia de <see cref="ILoggerService{TCategoryName}"/>.</returns>
        /// <exception cref="ArgumentException">Si no se encuentra un proveedor con el nombre especificado.</exception>
        ILoggerService<TCategoryName> GetLogger<TCategoryName>(string providerName);

        /// <summary>
        /// Obtiene una instancia de <see cref="ILoggerService{TCategoryName}"/> utilizando el proveedor por defecto
        /// o el primer proveedor registrado si no se especifica uno por defecto.
        /// </summary>
        /// <typeparam name="TCategoryName">El tipo usado para la categoría del logger.</typeparam>
        /// <returns>Una instancia de <see cref="ILoggerService{TCategoryName}"/>.</returns>
        /// <exception cref="InvalidOperationException">Si no hay proveedores registrados.</exception>
        ILoggerService<TCategoryName> GetLogger<TCategoryName>();
    }
}
