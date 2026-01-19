using KUtilitiesCore.DataAccess.EfCore.Evaluators;
using KUtilitiesCore.DataAccess.UOW.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.DataAccess.EfCore.Repositories
{
    /// <summary>
    /// Implementación concreta de IRepository solo lectura, usando Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>

    public class EfRepository<T> : EfRepositoryReadOnly<T>, IRepository<T>
        where T : class
    {
        #region Constructors

        public EfRepository(DbContext dbContext) : base(dbContext)
        { }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public void AddEntity(T entity)
        {
            _dbSet.Add(entity);
        }

        /// <inheritdoc/>
        public void DeleteEntity(T entity)
        { _dbSet.Remove(entity); }

        /// <inheritdoc/>
        public void UpdateEntity(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        #endregion Methods
    }
    /// <summary>
    /// Implementación concreta de IRepository usando Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public class EfRepositoryReadOnly<T> : IRepositoryReadOnly<T>
            where T : class
    {
        #region Fields

        protected readonly DbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        #endregion Fields

        #region Constructors

        public EfRepositoryReadOnly(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public int Count(ISpecification<T> spec)
        {
            return ApplySpecification(spec).Count();
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetEntities(ISpecification<T> spec)
        {
            return ApplySpecification(spec).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        /// <inheritdoc/>
        public T? GetFirstOrDefault(ISpecification<T> spec)
        {
            return ApplySpecification(spec).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<T?> GetFirstOrDefaultAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Método auxiliar para aplicar la especificación sobre el DbSet actual.
        /// </summary>
        internal IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
        }

        #endregion Methods
    }
}