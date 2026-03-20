using Microsoft.Extensions.Logging;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Define un contrato específico para servicios de logging que persisten en SQL Server.
    /// </summary>
    /// <typeparam name="TCategoryName">Categoría del logger.</typeparam>
    public interface ISqlLoggerService<out TCategoryName> : ILoggerService<TCategoryName>
    {
        /// <summary>
        /// Asegura que la infraestructura de base de datos (tabla) esté lista.
        /// </summary>
        void EnsureDatabaseCreated();
    }
}
