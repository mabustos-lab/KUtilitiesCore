using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Reflection;
using KUtilitiesCore.Logger;
using KUtilitiesCore.Dal.Paging;


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
#pragma warning disable CRR0029

namespace KUtilitiesCore.DataAccess.UOW
{
    /// <summary> Implementación base genérica del repositorio que implementa IRepository<TEntity>.
    /// Ya no utiliza TPrimaryKey. </summary> <typeparam name="TEntity">El tipo de la
    /// entidad.</typeparam> <typeparam name="TDbContext">El tipo del DbContext específico (EF6 o EF Core).</typeparam>
    public abstract class EfRepositoryBase<TEntity, TDbContext> : IRepository<TEntity> where TEntity : class
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
        protected EfRepositoryBase(TDbContext context, ILoggerServiceFactory loggerFactory = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Context.Set<TEntity>();
            Logger = loggerFactory?.GetLogger<EfRepositoryBase<TEntity, TDbContext>>() ?? NullLoggerService<EfRepositoryBase<TEntity, TDbContext>>.Instance;
        }

        // --- Implementación de IReadOnlyDbRepository<TEntity> ---
        /// <inheritdoc/>
        public virtual async Task<TEntity> FindOneAsync(ISpecification<TEntity> specification = null)
        {
            specification ??= Specification<TEntity>.Empty;
            IQueryable<TEntity> query = ApplySpecification(specification);
            return await query.SingleOrDefaultAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<TEntity>> FindAsync(ISpecification<TEntity> specification = null)
        {
            specification ??= Specification<TEntity>.Empty;
            IQueryable<TEntity> query = ApplySpecification(specification);
            return await query.ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<IPagedResult<TEntity>> GetPagedAsync(
            IPagingOptions pagingOptions,
            ISpecification<TEntity> specification = null)
        {
            if (pagingOptions == null)
                throw new ArgumentNullException(nameof(pagingOptions));
            specification ??= Specification<TEntity>.Empty;
            if (!pagingOptions.SkipPagination && pagingOptions.PageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pagingOptions.PageNumber));
            if (!pagingOptions.SkipPagination && pagingOptions.PageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pagingOptions.PageSize));

            IQueryable<TEntity> query = ApplySpecification(specification);
            List<TEntity> items;
            int totalCount = -1;
            bool hasNextPage;
            object lastKeyValue = null;
            int currentPageNumber = pagingOptions.PageNumber;

            if (pagingOptions.SkipPagination)
            {
                items = await query.ToListAsync();
                totalCount = items.Count;
                hasNextPage = false;
                currentPageNumber = 1;
                if (!items.IsNullOrEmpty() && (specification.OrderBy != null || specification.OrderByDescending != null))
                {
                    lastKeyValue = GetKeyValueFromEntity(items.Last(), specification);
                }
                return new PagedResult<TEntity>(
                    items,
                    totalCount,
                    currentPageNumber,
                    totalCount > 0 ? totalCount : 0,
                    lastKeyValue,
                    hasNextPage);
            }

            if (pagingOptions.Strategy == PagingStrategy.Offset)
            {
                totalCount = await query.CountAsync();
                int skip = (pagingOptions.PageNumber - 1) * pagingOptions.PageSize;

                items = await query.Skip(skip).Take(pagingOptions.PageSize).ToListAsync();

                hasNextPage = (pagingOptions.PageNumber * pagingOptions.PageSize) < totalCount;
                if (items.Any() && (specification.OrderBy != null || specification.OrderByDescending != null))
                {
                    lastKeyValue = GetKeyValueFromEntity(items.Last(), specification);
                }
            }
            else // PagingStrategy.Keyset
            {
                if (specification.OrderBy == null && specification.OrderByDescending == null)
                {
                    throw new InvalidOperationException(
                        "La paginación Keyset requiere una cláusula OrderBy u OrderByDescending en la especificación.");
                }
                if (pagingOptions.AfterValue != null)
                {
                    query = ApplyKeysetFilter(query, specification, pagingOptions.AfterValue);
                }

                var itemsWithNext = await query.Take(pagingOptions.PageSize + 1).ToListAsync();

                hasNextPage = itemsWithNext.Count > pagingOptions.PageSize;
                items = itemsWithNext.Take(pagingOptions.PageSize).ToList();
                if (items.Any())
                {
                    lastKeyValue = GetKeyValueFromEntity(items.Last(), specification);
                }
                totalCount = -1;
                currentPageNumber = pagingOptions.AfterValue == null
                    ? 1
                    : (pagingOptions.PageNumber > 0 ? pagingOptions.PageNumber : 1);
            }
            return new PagedResult<TEntity>(
                items,
                totalCount,
                currentPageNumber,
                pagingOptions.PageSize,
                lastKeyValue,
                hasNextPage);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ExistsAsync(ISpecification<TEntity> specification = null)
        {
            specification ??= Specification<TEntity>.Empty;
            return await ApplySpecification(specification).AnyAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(ISpecification<TEntity> specification = null)
        {
            specification ??= Specification<TEntity>.Empty;
            return await ApplySpecification(specification).CountAsync();
        }

        // --- Implementación de IRepository<TEntity> (métodos de escritura) ---
        /// <inheritdoc/>
        public virtual Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            DbSet.Add(entity);
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            DbSet.AddRange(entities);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
                DbSet.Attach(entity);
            entry.State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task DeleteAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            var entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
                DbSet.Attach(entity);
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        // GetByIdAsync(TPrimaryKey id) y DeleteAsync(TPrimaryKey id) han sido eliminados de
        // IRepository<TEntity> y por lo tanto de esta clase base. Las operaciones por ID ahora se
        // manejan a través de especificaciones.

        /// <summary>
        /// Aplica una especificación a un IQueryable base.
        /// </summary>
        protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            return SpecificationEvaluator<TEntity>.GetQuery(DbSet.AsQueryable(), specification);
        }

        /// <summary>
        /// Aplica el filtro para la paginación Keyset.
        /// </summary>
        protected virtual IQueryable<TEntity> ApplyKeysetFilter(
            IQueryable<TEntity> query,
            ISpecification<TEntity> specification,
            object afterValue)
        {
            if (afterValue == null)
                return query;
            LambdaExpression orderByExpression = specification.OrderBy ?? specification.OrderByDescending;
            if (orderByExpression == null)
                throw new InvalidOperationException("Keyset pagination requiere OrderBy/OrderByDescending.");

            var parameter = orderByExpression.Parameters.Single();
            var propertyAccess = orderByExpression.Body;
            if (propertyAccess is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
                propertyAccess = unary.Operand;
            if (!(propertyAccess is MemberExpression memberExpression))
                throw new ArgumentException("Expresión OrderBy para Keyset debe ser un acceso a miembro.");

            ConstantExpression constantValue;
            try
            {
                var propertyType = ((PropertyInfo)memberExpression.Member).PropertyType;
                var convertedAfterValue = Convert.ChangeType(afterValue, propertyType);
                constantValue = Expression.Constant(convertedAfterValue, propertyType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Tipo de AfterValue incompatible. AfterValue: '{afterValue}'.",
                    nameof(afterValue),
                    ex);
            }

            BinaryExpression comparison = specification.OrderBy != null
                ? Expression.GreaterThan(propertyAccess, constantValue)
                : Expression.LessThan(propertyAccess, constantValue);
            var keysetPredicate = Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
            return query.Where(keysetPredicate);
        }

        /// <summary>
        /// Obtiene el valor de la propiedad de ordenación de una entidad.
        /// </summary>
        protected virtual object GetKeyValueFromEntity(TEntity entity, ISpecification<TEntity> specification)
        {
            if (entity == null)
                return null;
            LambdaExpression orderByExpression = specification.OrderBy ?? specification.OrderByDescending;
            if (orderByExpression == null)
                return null;
            try
            {
                return orderByExpression.Compile().DynamicInvoke(entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al obtener valor clave de entidad para Keyset.");
                return null;
            }
        }
    }

#pragma warning restore CRR0029
}