using KUtilitiesCore.DataAccess.EfCore.Evaluators;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Inicializa el repositorio de lectura/escritura con el contexto de EF Core indicado.
        /// </summary>
        /// <param name="dbContext">Contexto de EF Core utilizado para todas las operaciones del repositorio.</param>
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

        /// <summary>
        /// Contexto de EF Core compartido por las operaciones del repositorio.
        /// </summary>
        protected readonly DbContext _dbContext;

        /// <summary>
        /// Conjunto de entidades (<see cref="DbSet{TEntity}"/>) sobre el que se ejecutan las consultas.
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa el repositorio de solo lectura, obteniendo el <see cref="DbSet{TEntity}"/> de la entidad <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dbContext">Contexto de EF Core utilizado para todas las operaciones de lectura.</param>
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
        /// <remarks>
        /// Devuelve <see langword="null"/> cuando ninguna entidad cumple la especificación;
        /// se anota con <see cref="MaybeNullAttribute"/> para alinearse con el contrato oblivious de la interfaz.
        /// </remarks>
        [return: MaybeNull]
        public async Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec)
        {
            // FirstOrDefaultAsync devuelve null cuando no hay coincidencias; el contrato
            // [return: MaybeNull] expresa esa posibilidad, pero el análisis de cuerpo de
            // métodos async no tiene en cuenta MaybeNull, y Task<T> está impuesto por la
            // interfaz IRepositoryReadOnly<T> (contexto de nulabilidad deshabilitado).
            return (await ApplySpecification(spec).FirstOrDefaultAsync())!;
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