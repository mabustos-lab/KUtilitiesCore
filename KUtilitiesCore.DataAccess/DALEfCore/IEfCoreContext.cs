using KUtilitiesCore.DataAccess.DAL;

// --- Compilación Condicional para Entity Framework ---
#if NETFRAMEWORK

// Usings específicos de Entity Framework 6 (.NET Framework)
using System.Data.Entity;

#elif NETCOREAPP
// Usings específicos de Entity Framework Core (.NET Core)
using Microsoft.EntityFrameworkCore;
#endif

namespace KUtilitiesCore.DataAccess.DALEfCore
{
    /// <summary>
    /// Interfaz específica para Entity Framework Core que extiende las capacidades de ejecución SQL
    /// y proporciona acceso directo al <see cref="DbContext"/> subyacente. Permite realizar
    /// operaciones avanzadas sobre el contexto de datos de EF Core, incluyendo la ejecución de
    /// comandos SQL y la gestión de transacciones.
    /// </summary>
    public interface IEfCoreContext : ISqlExecutorContext
    {
        /// <summary>
        /// Obtiene la instancia actual de <see cref="DbContext"/> asociada al contexto. Permite
        /// acceder a las funcionalidades nativas de Entity Framework Core, como el seguimiento de
        /// entidades, la configuración de modelos y la ejecución de consultas LINQ.
        /// </summary>
        DbContext Context { get; }
    }
}