using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.DataAccess.Utils;
using KUtilitiesCore.Logger;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;



// --- Compilación Condicional para Entity Framework ---
#if NETFRAMEWORK
// Usings específicos de Entity Framework 6 (.NET Framework)
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
#elif NETCOREAPP
// Usings específicos de Entity Framework Core (.NET Core)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query; // Para SetPropertyCalls

using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;
using DbUpdateConcurrencyException = Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
#else
#error "Target framework no soportado. Defina NETFRAMEWORK o NETCOREAPP."
#endif

namespace KUtilitiesCore.DataAccess.UOW
{
    //    /// <summary>
    //    /// Implementación del repositorio de Productos.
    //    /// </summary>
    //    public class EfProductRepository<TDbContext>
    //        : EfCoreRepositoryBase<Product, TDbContext>, IProductRepository // Asume que IProductRepository hereda de IEfCoreRepository<Product> o IRepository<Product>
    //        where TDbContext : IDisposable
    //#if NETFRAMEWORK
    //        , System.Data.Entity.DbContext
    //#elif NETCOREAPP
    //        , Microsoft.EntityFrameworkCore.DbContext
    //#endif
    //    {
    //        public EfProductRepository(TDbContext context, ILoggerServiceProvider loggerFactory = null)
    //            : base(context, loggerFactory) { }

    //        public async Task<IPagedResult<Product>> GetProductsByCategoryPagedAsync(int categoryId, IPagingOptions pagingOptions)
    //        {
    //            Expression<Func<Product, bool>> criteria = p => p.CategoryId == categoryId;
    //            var spec = new AnonymousSpecification<Product>(criteria);
    //            // Es importante definir una ordenación para que Keyset funcione consistentemente
    //            spec.ApplyOrderBy(p => p.Id); // O p.Name, o cualquier otra propiedad adecuada
    //            return await GetPagedAsync(pagingOptions, spec);
    //        }
    //        private class AnonymousSpecification<T> : Specification<T> where T : class { public AnonymousSpecification(Expression<Func<T, bool>> criteria) : base(criteria) { } }
    //    }

    /// <summary>
    /// Implementación base de la Unidad de Trabajo.
    /// </summary>
    public abstract class EfUnitOfWorkBase<TDbContext> : IUnitOfWork
        where TDbContext :
#if NETFRAMEWORK
         DbContext
#elif NETCOREAPP
         DbContext
#endif
        , IDisposable
    {
        protected readonly TDbContext Context;
        protected readonly ILoggerService Logger;
        private bool _disposed = false;
#if NETCOREAPP
        private IDbContextTransaction _currentTransaction;
#endif
        private Dictionary<Type, object> _repositories;
        protected readonly ILoggerServiceProvider LoggerFactoryInternal;

        protected EfUnitOfWorkBase(TDbContext context, ILoggerServiceProvider loggerFactory = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            LoggerFactoryInternal = loggerFactory;
            Logger = loggerFactory?.CreateLogger<EfUnitOfWorkBase<TDbContext>>() ?? NullLoggerService<EfUnitOfWorkBase<TDbContext>>.Instance;
        }

        protected TRepoInterface ResolveRepository<TRepoInterface, TRepoImplementation>()
            where TRepoInterface : class
            where TRepoImplementation : class, TRepoInterface
        {
            if (_repositories == null) _repositories = new Dictionary<Type, object>();
            var repoType = typeof(TRepoInterface);
            if (!_repositories.ContainsKey(repoType))
            {
                var instance = Activator.CreateInstance(typeof(TRepoImplementation), Context, LoggerFactoryInternal) as TRepoImplementation;
                if (instance == null) throw new InvalidOperationException($"No se pudo crear instancia de {typeof(TRepoImplementation).Name}.");
                _repositories[repoType] = instance;
            }
            return (TRepoInterface)_repositories[repoType];
        }

        public virtual IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories == null) _repositories = new Dictionary<Type, object>();
            var entityType = typeof(TEntity);
            if (!_repositories.ContainsKey(entityType))
            {
                var repositoryType = typeof(EfGenericRepository<,>); // Ahora solo TEntity, TDbContext
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity), typeof(TDbContext)), // Ajustado
                    Context, LoggerFactoryInternal);
                _repositories.Add(entityType, repositoryInstance);
                return (IRepository<TEntity>)repositoryInstance;
            }
            return (IRepository<TEntity>)_repositories[entityType];
        }

        public virtual IEfCoreRepository<TEntity> GetEfCoreRepository<TEntity>() where TEntity : class
        {
#if NETCOREAPP
            if (_repositories == null) _repositories = new Dictionary<Type, object>();
            var efCoreRepoKey = typeof(IEfCoreRepository<TEntity>);
            if (!_repositories.ContainsKey(efCoreRepoKey))
            {
                var repositoryType = typeof(EfCoreGenericRepository<,>); // Ahora solo TEntity, TDbContext
                var repositoryInstance = Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(TEntity), typeof(TDbContext)), // Ajustado
                    Context, LoggerFactoryInternal);

                if (repositoryInstance is IEfCoreRepository<TEntity> efCoreRepo)
                {
                    _repositories.Add(efCoreRepoKey, efCoreRepo);
                    return efCoreRepo;
                }
                else
                {
                    throw new InvalidOperationException($"No se pudo obtener IEfCoreRepository para {typeof(TEntity).Name}.");
                }
            }
            return (IEfCoreRepository<TEntity>)_repositories[efCoreRepoKey];
#else
            throw new PlatformNotSupportedException("IEfCoreRepository solo está disponible en .NET Core con EF Core.");
#endif
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            Logger.LogInformation("Iniciando SaveChangesAsync.");
            int result = 0;
#if NETFRAMEWORK
            using (var dbContextTransaction = (Context as DbContext).Database.BeginTransaction())
            {
                try
                {
                    result = await (Context as DbContext).SaveChangesAsync();
                    dbContextTransaction.Commit();
                    Logger.LogInformation("SaveChangesAsync completado. {Count} cambios guardados. Transacción confirmada.", result);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Logger.LogError(ex, "Error de concurrencia durante SaveChangesAsync. Rollback.");
                    dbContextTransaction.Rollback();
                    throw new ConcurrencyException("Conflicto de concurrencia detectado al guardar.", ex);
                }
                catch (DbEntityValidationException ex)
                {
                    Logger.LogError(ex, "Error de validación durante SaveChangesAsync. Rollback.");
                    foreach (var validationErrors in ex.EntityValidationErrors)
                        foreach (var validationError in validationErrors.ValidationErrors)
                            Logger.LogError("- Propiedad: {Property}, Error: {Error}", validationError.PropertyName, validationError.ErrorMessage);
                    dbContextTransaction.Rollback();
                    throw new RepositoryException("Error de validación al guardar cambios.", ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error inesperado durante SaveChangesAsync. Rollback.");
                    dbContextTransaction.Rollback();
                    throw new RepositoryException("Error inesperado al guardar cambios.", ex);
                }
            }
#elif NETCOREAPP
            var efCoreContext = Context as DbContext;
            _currentTransaction = _currentTransaction ?? await efCoreContext.Database.BeginTransactionAsync();
            try { result = await efCoreContext.SaveChangesAsync(); await _currentTransaction.CommitAsync(); }
            catch (DbUpdateConcurrencyException ex) { await RollbackTransactionAsync(); throw new ConcurrencyException("Conflicto de concurrencia.", ex); }
            catch (DbUpdateException ex) { await RollbackTransactionAsync(); throw new RepositoryException("Error al actualizar DB.", ex); }
            catch (Exception ex) { await RollbackTransactionAsync(); throw new RepositoryException("Error inesperado.", ex); }
            finally { if (_currentTransaction != null) { await _currentTransaction.DisposeAsync(); _currentTransaction = null; } }
#endif
            return result;
        }
#if NETCOREAPP
        private async Task RollbackTransactionAsync() 
        {
            try
            {
                if (_currentTransaction != null) await _currentTransaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                Logger.LogError(rollbackEx, "Error catastrófico: Falló el Rollback de la transacción.");
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
#endif
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing) { if (!_disposed) { if (disposing) { Context?.Dispose(); } _disposed = true; } }
    }
}
