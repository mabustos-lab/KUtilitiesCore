using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Implementación de repositorio genérico para acceso a datos nativo (ADO.NET/Dapper) usando IDAOContext.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public abstract class DaoRepository<T> : DaoRepositoryReadOnly<T>, IRepository<T>
        where T : class, new()
    {
        #region Constructors

        protected DaoRepository(IDaoUowContext context) : base(context)
        { }

        #endregion Constructors

        #region Methods

        /// <inheritdoc/>
        public abstract void AddEntity(T entity);

        /// <inheritdoc/>
        public abstract void DeleteEntity(T entity);

        /// <inheritdoc/>
        public abstract void UpdateEntity(T entity);

        #endregion Methods
    }

    /// <summary>
    /// Implementación de repositorio de solo lectura genérico para acceso a datos nativo
    /// (ADO.NET/Dapper) usando IDAOContext.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public abstract class DaoRepositoryReadOnly<T> : IRepositoryReadOnly<T>
        where T : class, new()
    {
        #region Fields

        private readonly IDaoUowContext _uowContext;

        #endregion Fields

        #region Constructors

        protected DaoRepositoryReadOnly(IDaoUowContext context)
        {
            _uowContext = context;
            TableName = typeof(T).Name;
        }

        #endregion Constructors

        #region Properties

        // Propiedad para definir el nombre de la tabla o SP base si fuera necesario
        internal string TableName { get; }

        /// <summary>
        /// Acceso directo al contexto de datos (atajo).
        /// </summary>
        protected IDaoContext Context => _uowContext.Context;

        /// <summary>
        /// Acceso directo a la transacción (atajo).
        /// </summary>
        protected ITransaction Transaction => _uowContext.Transaction;
        /// <summary>
        /// Accesos directo a las funciones para obtener los repositorios
        /// </summary>
        protected IDaoRepositoryProvider RepositoryProvider => _uowContext.DaoRepositoryProvider;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public abstract int Count(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<int> CountAsync(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract IEnumerable<T> GetEntities(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract T GetFirstOrDefault(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec);

        /// <summary>
        /// Convierte el diccionario de la especificación a la colección nativa de parámetros del DAL.
        /// </summary>
        protected IDaoParameterCollection MapParameters(IDictionary<string, object> specParams)
        {
            var collection = _uowContext.Context.CreateParameterCollection();
            if (specParams != null)
            {
                foreach (var param in specParams)
                {
                    collection.Add(param.Key, param.Value);
                }
            }
            return collection;
        }

        #endregion Methods
    }

    internal class DefaultDaoRepository<T> : DaoRepository<T>
        where T : class, new()
    {
        public DefaultDaoRepository(IDaoUowContext context) : base(context)
        {
        }

        public override void AddEntity(T entity)
        {
            throw new NotImplementedException();
        }

        public override int Count(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override Task<int> CountAsync(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override void DeleteEntity(T entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> GetEntities(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override T GetFirstOrDefault(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec)
        {
            throw new NotImplementedException();
        }

        public override void UpdateEntity(T entity)
        {
            throw new NotImplementedException();
        }
    }
}