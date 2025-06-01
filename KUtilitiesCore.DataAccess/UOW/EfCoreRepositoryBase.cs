using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using KUtilitiesCore.DataAccess.Utils;
using System.Linq.Expressions;
using System.Reflection;
using KUtilitiesCore.DataAccess.UOW.Interfaces;

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
    /// <summary>
    /// Implementación base para repositorios con capacidades específicas de EF Core.
    /// </summary>
    public abstract class EfCoreRepositoryBase<TEntity, TDbContext>
        : EfRepositoryBase<TEntity, TDbContext>, IEfCoreRepository<TEntity>
        where TEntity : class
#if NETFRAMEWORK
        where TDbContext : DbContext
#elif NETCOREAPP
        where TDbContext : DbContext
#endif
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected EfCoreRepositoryBase(TDbContext context, ILoggerFactory loggerFactory = null)
            : base(context, loggerFactory) { }

#if NETCOREAPP
        /// <inheritdoc />
        public virtual async Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification, IEnumerable<PropertyUpdateDescriptor<TEntity>> updates)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (updates == null) throw new ArgumentNullException(nameof(updates));
            if (!updates.Any()) return 0;

            IQueryable<TEntity> query = ApplySpecification(specification);
            var setPropertyCallsParameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "s");
            Expression body = setPropertyCallsParameter;

            foreach (var update in updates)
            {
                var propertySelectorConverted = update.PropertySelector;
                var valueExpressionConverted = update.ValueExpression;
                var propertyLambdaType = propertySelectorConverted.GetType();
                var propertyGenericArgument = propertyLambdaType.GetGenericArguments()[0];
                var returnPropertyType = propertyGenericArgument.GetGenericArguments()[1];

                MethodInfo setPropertyMethod = typeof(SetPropertyCalls<TEntity>)
                    .GetMethod("SetProperty", new[] {
                        typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(typeof(TEntity), returnPropertyType)),
                        typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(typeof(TEntity), returnPropertyType))
                    })
                    ?.MakeGenericMethod(returnPropertyType);

                if (setPropertyMethod == null) throw new InvalidOperationException("No se encontró el método SetProperty adecuado.");
                body = Expression.Call(body, setPropertyMethod, propertySelectorConverted, valueExpressionConverted);
            }
            if (body == setPropertyCallsParameter) return 0;
            var updateLambda = Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(body, setPropertyCallsParameter);
            return await query.ExecuteUpdateAsync(updateLambda);
        }

        /// <inheritdoc />
        public virtual async Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            IQueryable<TEntity> query = ApplySpecification(specification);
            return await query.ExecuteDeleteAsync();
        }

        /// <inheritdoc />
        public virtual int ExecuteDelete(ISpecification<TEntity> specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            IQueryable<TEntity> query = ApplySpecification(specification);
            return query.ExecuteDelete();
        }
#else
        public virtual Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification, IEnumerable<PropertyUpdateDescriptor<TEntity>> updates)
        { throw new PlatformNotSupportedException("ExecuteUpdateAsync solo es compatible con EF Core 7.0+."); }
        public virtual Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification)
        { throw new PlatformNotSupportedException("ExecuteDeleteAsync solo es compatible con EF Core 7.0+."); }
        public virtual int ExecuteDelete(ISpecification<TEntity> specification)
        { throw new PlatformNotSupportedException("ExecuteDelete (síncrono) solo es compatible con EF Core 7.0+."); }
#endif
    }
}
