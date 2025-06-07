using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.Logger.Providers
{
    /// <summary>
    /// Proveedor para crear instancias de <see cref="NullLoggerService{TCategoryName}"/>,
    /// un logger que no realiza ninguna acción.
    /// </summary>
    public class NullLoggerServiceProvider : ILoggerServiceProvider
    {
        /// <inheritdoc/>
        public string Name => nameof(NullLoggerServiceProvider); // Nombre único para este proveedor
        /// <inheritdoc/>
        public ILoggerService<TCategoryName> CreateLogger<TCategoryName>()
        {
            return NullLoggerService<TCategoryName>.Instance;
        }
    }
}
