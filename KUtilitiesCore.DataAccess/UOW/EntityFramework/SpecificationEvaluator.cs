using KUtilitiesCore.DataAccess.UOW.Interfaces;
using KUtilitiesCore.Logger;
// --- Compilación Condicional para Entity Framework ---
#if NETFRAMEWORK
// Usings específicos de Entity Framework 6 (.NET Framework)
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
#elif NETCOREAPP
// Usings específicos de Entity Framework Core (.NET Core)
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; // Para IDbContextTransaction
using DbUpdateException = Microsoft.EntityFrameworkCore.DbUpdateException;
using DbUpdateConcurrencyException = Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;
#else
#error "Target framework no soportado. Defina NETFRAMEWORK o NETCOREAPP."
#endif

namespace KUtilitiesCore.DataAccess.UOW.EntityFramework
{
    /// <summary>
    /// Clase responsable de aplicar una especificación (ISpecification) a un IQueryable.
    /// La paginación no se maneja aquí, sino externamente.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad.</typeparam>
    public static class SpecificationEvaluator<TEntity> where TEntity : class
    {
        /// <summary>
        /// Aplica la especificación dada (criterios, includes, ordenación) a la consulta IQueryable de entrada.
        /// </summary>
        /// <param name="inputQuery">La consulta IQueryable de entrada.</param>
        /// <param name="specification">La especificación a aplicar.</param>
        /// <returns>El IQueryable con la especificación aplicada.</returns>
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            // Aplicar criterio de filtrado
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Aplicar inclusiones (Includes)
            if (specification.Includes != null)
            {
                query = specification.Includes.Aggregate(query,
                                        (current, include) => current.Include(include));
            }

            // Aplicar inclusiones (IncludeStrings)
            if (specification.IncludeStrings != null)
            {
                query = specification.IncludeStrings.Aggregate(query,
                                        (current, include) => current.Include(include));
            }

            // Aplicar ordenación
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }
            return query;
        }
    }

    /// <summary>
    /// Implementación base genérica del repositorio utilizando Entity Framework y el patrón Specification.
    /// Implementa IRepository<TEntity, TPrimaryKey> (que hereda de IReadOnlyDbRepository<TEntity>).
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad.</typeparam>
    /// <typeparam name="TPrimaryKey">El tipo de la clave primaria.</typeparam>
    /// <typeparam name="TDbContext">El tipo del DbContext específico (EF6 o EF Core).</typeparam>
    public abstract class EfRepositoryBase<TEntity, TPrimaryKey, TDbContext> : IRepository<TEntity, TPrimaryKey>
        where TEntity : class
#if NETFRAMEWORK
        where TDbContext : DbContext
#elif NETCOREAPP
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
#endif
    {
        protected readonly TDbContext Context;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly ILoggerService Logger;

        /// <summary>
        /// Constructor base.
        /// </summary>
        protected EfRepositoryBase(TDbContext context, ILoggerService loggerFactory = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Context.Set<TEntity>();
            Logger = loggerFactory;
            Logger.LogDebug("Repositorio {RepositoryType} inicializado con DbContext {DbContextType}", GetType().Name, typeof(TDbContext).Name);
        }

        // --- Métodos de IReadOnlyDbRepository<TEntity> ---

        /// <inheritdoc />
        public virtual async Task<TEntity> FindOneAsync(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            Logger.LogDebug("Buscando una única {Entity} con especificación: {SpecificationType}", typeof(TEntity).Name, specification.GetType().Name);
            try
            {
                IQueryable<TEntity> query = ApplySpecification(specification);
#if NETFRAMEWORK
                return await query.SingleOrDefaultAsync();
#elif NETCOREAPP
                return await query.SingleOrDefaultAsync();
#endif
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogError(ex, "Error en FindOneAsync: Más de una {Entity} encontrada que cumple la especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Más de una {typeof(TEntity).Name} encontrada que cumple la especificación.", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error en FindOneAsync buscando {Entity} con especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Error buscando una única {typeof(TEntity).Name} con especificación.", ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<TEntity>> FindAsync(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            Logger.LogDebug("Buscando {Entity} con especificación: {SpecificationType}", typeof(TEntity).Name, specification.GetType().Name);
            try
            {
                // FindAsync no aplica paginación por sí mismo.
                IQueryable<TEntity> query = ApplySpecification(specification);
#if NETFRAMEWORK
                return await query.ToListAsync();
#elif NETCOREAPP
                return await query.ToListAsync();
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error en FindAsync buscando {Entity} con especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Error buscando {typeof(TEntity).Name} con especificación.", ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<IPagedResult<TEntity>> GetPagedAsync(IPagingOptions pagingOptions, ISpecification<TEntity> specification)
        {
            if (pagingOptions == null) throw new ArgumentNullException(nameof(pagingOptions));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            // Validaciones básicas de paginación (pueden ajustarse si SkipPagination es true)
            if (!pagingOptions.SkipPagination && pagingOptions.PageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pagingOptions.PageNumber), "PageNumber debe ser > 0 cuando la paginación está activa.");
            if (!pagingOptions.SkipPagination && pagingOptions.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pagingOptions.PageSize), "PageSize debe ser > 0 cuando la paginación está activa.");


            Logger.LogDebug("Obteniendo resultados para {Entity} con especificación: {SpecificationType}. Opciones Paginación: Skip={SkipPagination}, Page={PageNumber}, Size={PageSize}",
                           typeof(TEntity).Name, specification.GetType().Name, pagingOptions.SkipPagination, pagingOptions.PageNumber, pagingOptions.PageSize);
            try
            {
                // 1. Aplicar la especificación (filtros, includes, ordenación)
                IQueryable<TEntity> query = ApplySpecification(specification);

                // 2. Obtener el conteo total ANTES de aplicar Skip/Take
                int totalCount;
#if NETFRAMEWORK
                totalCount = await query.CountAsync();
#elif NETCOREAPP
                totalCount = await query.CountAsync();
#endif

                // 3. Aplicar paginación (Skip y Take) basada en IPagingOptions, si no se omite
                IQueryable<TEntity> pagedQuery = query; // Empezar con la consulta filtrada/ordenada
                int pageNumber = pagingOptions.PageNumber;
                int pageSize = pagingOptions.PageSize;

                if (!pagingOptions.SkipPagination)
                {
                    int skip = (pageNumber - 1) * pageSize;
                    pagedQuery = query.Skip(skip).Take(pageSize);
                    Logger.LogTrace("Paginación aplicada: Skip={SkipCount}, Take={TakeCount}", skip, pageSize);
                }
                else
                {
                    // Si se omite la paginación, ajustamos los parámetros para PagedResult
                    pageNumber = 1; // Solo hay una página (todos los resultados)
                    pageSize = totalCount; // El tamaño de la página es el total de elementos
                    Logger.LogTrace("Paginación omitida (SkipPagination=true).");
                }


                // 4. Ejecutar la consulta para obtener los items
#if NETFRAMEWORK
                List<TEntity> items = await pagedQuery.ToListAsync();
#elif NETCOREAPP
                List<TEntity> items = await pagedQuery.ToListAsync();
#endif
                // Asegurar que pageSize no sea 0 si totalCount > 0 para el constructor de PagedResult
                int finalPageSize = (pageSize <= 0 && totalCount > 0) ? totalCount : pageSize;

                return new PagedResult<TEntity>(items, totalCount, pageNumber, finalPageSize);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error en GetPagedAsync obteniendo página para {Entity} con especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Error obteniendo resultado paginado para {typeof(TEntity).Name} con especificación.", ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> ExistsAsync(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            Logger.LogDebug("Comprobando existencia de {Entity} con especificación: {SpecificationType}", typeof(TEntity).Name, specification.GetType().Name);
            try
            {
#if NETFRAMEWORK
                return await ApplySpecification(specification).AnyAsync();
#elif NETCOREAPP
                return await ApplySpecification(specification).AnyAsync();
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error comprobando existencia de {Entity} con especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Error comprobando existencia de {typeof(TEntity).Name} con especificación.", ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            Logger.LogDebug("Contando {Entity} con especificación: {SpecificationType}", typeof(TEntity).Name, specification.GetType().Name);
            try
            {
#if NETFRAMEWORK
                return await ApplySpecification(specification).CountAsync();
#elif NETCOREAPP
                return await ApplySpecification(specification).CountAsync();
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error contando {Entity} con especificación {SpecificationType}.", typeof(TEntity).Name, specification.GetType().Name);
                throw new RepositoryException($"Error contando {typeof(TEntity).Name} con especificación.", ex);
            }
        }

        // --- Métodos de IRepository<TEntity, TPrimaryKey> ---

        /// <inheritdoc />
        public virtual async Task<TEntity> GetByIdAsync(TPrimaryKey id)
        {
            Logger.LogDebug("Obteniendo {Entity} por ID: {Id}", typeof(TEntity).Name, id);
            try
            {
#if NETFRAMEWORK
                return await DbSet.FindAsync(id);
#elif NETCOREAPP
                return await DbSet.FindAsync(id);
#endif
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error obteniendo {Entity} por ID {Id}", typeof(TEntity).Name, id);
                throw new RepositoryException($"Error obteniendo {typeof(TEntity).Name} por ID {id}.", ex);
            }
        }

        /// <inheritdoc />
        public virtual Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Logger.LogDebug("Añadiendo nueva {Entity}.", typeof(TEntity).Name);
            try
            {
#if NETFRAMEWORK
                DbSet.Add(entity);
#elif NETCOREAPP
                DbSet.Add(entity);
#endif
                return Task.FromResult(entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error añadiendo {Entity}.", typeof(TEntity).Name);
                throw new RepositoryException($"Error añadiendo {typeof(TEntity).Name}.", ex);
            }
        }

        /// <inheritdoc />
        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            var entityList = entities as List<TEntity> ?? entities.ToList();
            if (!entityList.Any()) return Task.CompletedTask;

            Logger.LogDebug("Añadiendo rango de {Count} entidades {Entity}.", entityList.Count, typeof(TEntity).Name);
            try
            {
#if NETFRAMEWORK
                DbSet.AddRange(entityList);
#elif NETCOREAPP
                DbSet.AddRange(entityList);
#endif
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error añadiendo rango de {Entity}.", typeof(TEntity).Name);
                throw new RepositoryException($"Error añadiendo rango de {typeof(TEntity).Name}.", ex);
            }
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Logger.LogDebug("Actualizando {Entity}.", typeof(TEntity).Name);
            try
            {
                var entry = Context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    DbSet.Attach(entity);
                }
                entry.State = EntityState.Modified;
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error actualizando {Entity}.", typeof(TEntity).Name);
                throw new RepositoryException($"Error actualizando {typeof(TEntity).Name}.", ex);
            }
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Logger.LogDebug("Eliminando {Entity}.", typeof(TEntity).Name);
            try
            {
                var entry = Context.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    DbSet.Attach(entity);
                }
                DbSet.Remove(entity);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error eliminando {Entity}.", typeof(TEntity).Name);
                throw new RepositoryException($"Error eliminando {typeof(TEntity).Name}.", ex);
            }
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(TPrimaryKey id)
        {
            Logger.LogDebug("Eliminando {Entity} por ID: {Id}", typeof(TEntity).Name, id);
            try
            {
                TEntity entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    await DeleteAsync(entity);
                    return true;
                }
                Logger.LogWarning("No se encontró {Entity} con ID {Id} para eliminar.", typeof(TEntity).Name, id);
                return false;
            }
            catch (RepositoryException) { throw; }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error eliminando {Entity} por ID {Id}.", typeof(TEntity).Name, id);
                throw new RepositoryException($"Error eliminando {typeof(TEntity).Name} por ID {id}.", ex);
            }
        }

        // --- Métodos Protegidos ---

        /// <summary>
        /// Método protegido para aplicar una especificación (sin paginación) a un IQueryable base (DbSet).
        /// </summary>
        /// <param name="specification">La especificación a aplicar.</param>
        /// <returns>Un IQueryable con la especificación aplicada.</returns>
        protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            // Asegura que la especificación no sea null antes de pasarla al evaluador
            if (specification == null)
            {
                // Decide qué hacer: ¿devolver el DbSet sin filtrar o lanzar excepción?
                // Lanzar excepción es generalmente más seguro para evitar resultados inesperados.
                throw new ArgumentNullException(nameof(specification), "La especificación no puede ser nula.");
                // Alternativa: return DbSet.AsQueryable(); // Devuelve todo si la especificación es null
            }
            return SpecificationEvaluator<TEntity>.GetQuery(DbSet.AsQueryable(), specification);
        }
    }
}
