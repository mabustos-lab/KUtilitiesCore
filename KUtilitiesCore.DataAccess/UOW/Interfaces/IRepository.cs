using KUtilitiesCore.DataAccess.UOW.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz genérica de solo lectura de repositorio.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad (clase).</typeparam>
    public interface IRepositoryReadOnly<T> 
        where T : class
    {
        /// <summary>
        /// Obtiene una única entidad que cumpla con la especificación.
        /// </summary>
        T GetFirstOrDefault(ISpecification<T> spec) ;
        /// <summary>
        /// Obtiene una única entidad asíncronamente basada en la especificación.
        /// </summary>
        Task<T> GetFirstOrDefaultAsync(ISpecification<T> spec) ;
        /// <summary>
        /// Obtiene una lista de entidades basada en la especificación.
        /// </summary>
        IEnumerable<T> GetEntities(ISpecification<T> spec) ;
        /// <summary>
        /// Obtiene una lista asíncrona.
        /// </summary>
        Task<IEnumerable<T>> GetEntitiesAsync(ISpecification<T> spec) ;
        /// <summary>
        /// Obtiene el número de registros de la tabla
        /// </summary>
        int Count(ISpecification<T> spec);
        /// <summary>
        /// Obtiene el número de registros de la tabla, de manera asyncrona.
        /// </summary>
        Task<int> CountAsync(ISpecification<T> spec);
    }

    /// <summary>
    /// Interfaz genérica de repositorio.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad (clase).</typeparam>ram>
    public interface IRepository<T> : IRepositoryReadOnly<T>
        where T : class
    {
        /// <summary>
        /// Agrega una nueva entidad al repositorio.
        /// </summary>
        void AddEntity(T entity);
       
        /// <summary>
        /// Actualiza una entidad existente en el repositorio.
        /// </summary>
        void UpdateEntity(T entity);
        
        /// <summary>
        /// Elimina una entidad existente en el repositorio.
        /// </summary>
        void DeleteEntity(T entity);
        
    }
}
