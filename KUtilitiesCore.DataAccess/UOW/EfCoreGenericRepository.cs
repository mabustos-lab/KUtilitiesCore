using KUtilitiesCore.Logger;
using Microsoft.Extensions.Logging;

// --- Compilación Condicional para Entity Framework ---
#if NETFRAMEWORK
// Usings específicos de Entity Framework 6 (.NET Framework)
using System.Data.Entity;
#elif NETCOREAPP
// Usings específicos de Entity Framework Core (.NET Core)
using Microsoft.EntityFrameworkCore;
#else
#error "Target framework no soportado. Defina NETFRAMEWORK o NETCOREAPP."
#endif

namespace KUtilitiesCore.DataAccess.UOW
{
    /// <summary>
    /// Implementación genérica de IEfCoreRepository (sin TPrimaryKey).
    /// </summary>
    public class EfCoreGenericRepository<TEntity, TDbContext>
        : EfCoreRepositoryBase<TEntity, TDbContext>
        where TEntity : class
#if NETFRAMEWORK
        where TDbContext : DbContext
#elif NETCOREAPP
        where TDbContext : DbContext
#endif
    {
        public EfCoreGenericRepository(TDbContext context, ILoggerServiceProvider loggerFactory = null)
            : base(context, loggerFactory) { }
    }
}
