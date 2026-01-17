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
    /// Implementación de repositorio de solo lectura genérico para acceso a datos nativo (ADO.NET/Dapper) usando IDAOContext.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public abstract class DaoRepositoryReadOnly<T> : IRepositoryReadOnly<T>
        where T : class, new()
    {
        // Propiedad para definir el nombre de la tabla o SP base si fuera necesario
        internal string TableName { get; }

        internal readonly IDaoUowContext _uowContext;
        protected DaoRepositoryReadOnly(IDaoUowContext context)
        {
            _uowContext = context;
            TableName = typeof(T).Name;
        }

        /// <inheritdoc/>
        public abstract T GetFirstOrDefault(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract IEnumerable<T> GetEntities(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract int Count(ISpecification<T> spec);

        /// <inheritdoc/>
        public abstract Task<int> CountAsync(ISpecification<T> spec);
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
    }
    /// <summary>
    /// Implementación de repositorio genérico para acceso a datos nativo (ADO.NET/Dapper) usando IDAOContext.
    /// </summary>
    /// <typeparam name="T">Tipo de la entidad.</typeparam>
    public abstract class DaoRepository<T>:DaoRepositoryReadOnly<T> , IRepository<T>
        where T : class, new ()
    {
        protected DaoRepository(IDaoUowContext context) : base(context)
        {}

        /// <inheritdoc/>
        public abstract void AddEntity(T entity);
        /// <inheritdoc/>
        public abstract void DeleteEntity(T entity);
        /// <inheritdoc/>
        public abstract void UpdateEntity(T entity);
    }
}