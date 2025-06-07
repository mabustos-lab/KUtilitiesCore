using KUtilitiesCore.Logger.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Define un contrato para los proveedores de servicios de logging.
    /// Un proveedor es responsable de crear instancias de ILoggerService para un tipo específico de logger.
    /// </summary>
    public interface ILoggerServiceProvider
    {
        /// <summary>
        /// Obtiene el nombre único del proveedor. Utilizado para registrar y recuperar en la fábrica.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Crea una instancia de <see cref="ILoggerService{TCategoryName}"/>.
        /// </summary>
        /// <typeparam name="TCategoryName">El tipo usado para la categoría del logger.</typeparam>
        /// <returns>Una instancia de <see cref="ILoggerService{TCategoryName}"/>.</returns>
        ILoggerService<TCategoryName> CreateLogger<TCategoryName>();
    }

}
