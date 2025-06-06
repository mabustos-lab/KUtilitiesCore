using KUtilitiesCore.Logger.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Define un contrato para una fábrica de servicios de logging.
    /// La fábrica es responsable de crear instancias de <see cref="ILoggerService{TCategoryName}"/>,
    /// potencialmente utilizando uno o más <see cref="ILoggerServiceProvider"/>.
    /// </summary>
    public interface ILoggerServiceFactory : IDisposable
    {
        /// <summary>
        /// Agrega un proveedor de servicios de logging a la fábrica.
        /// La fábrica puede utilizar múltiples proveedores para crear loggers compuestos.
        /// </summary>
        /// <param name="provider">El proveedor de servicios de logging a agregar.</param>
        /// <exception cref="ArgumentNullException">Se lanza si el <paramref name="provider"/> es nulo.</exception>
        void AddProvider(ILoggerServiceProvider provider);

        /// <summary>
        /// Obtiene una instancia de <see cref="ILoggerService{TCategoryName}"/> para la categoría especificada.
        /// Si se han agregado múltiples proveedores, podría devolver un logger compuesto.
        /// </summary>
        /// <typeparam name="TCategoryName">El tipo (categoría) para el cual se obtiene el logger.</typeparam>
        /// <param name="options">Opciones opcionales para configurar el logger. Si múltiples proveedores son usados, estas opciones podrían aplicarse a todos o ser específicas del proveedor.</param>
        /// <returns>Una instancia de <see cref="ILoggerService{TCategoryName}"/>.</returns>
        ILoggerService<TCategoryName> GetLogger<TCategoryName>(ILoggerOptions? options = null) where TCategoryName : class;
    }
}
