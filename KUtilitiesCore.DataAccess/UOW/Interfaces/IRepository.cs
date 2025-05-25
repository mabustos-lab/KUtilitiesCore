using KUtilitiesCore.DataAccess.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz genérica para un repositorio de datos completo (lectura/escritura)
    /// utilizando el patrón de Especificación. Hereda de IReadOnlyDbRepository.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad gestionada por el repositorio.</typeparam>
    /// <typeparam name="TPrimaryKey">El tipo de la clave primaria de la entidad.</typeparam>
    public interface IRepository<TEntity> : IReadOnlyDbRepository<TEntity>
        where TEntity : class
    {

        /// <summary>
        /// Añade una nueva entidad al repositorio.
        /// </summary>
        /// <param name="entity">La entidad a añadir.</param>
        /// <returns>La entidad añadida (puede ser actualizada por el ORM, ej: con ID generada).</returns>
        TEntity Add(TEntity entity);

        /// <summary>
        /// Añade una colección de nuevas entidades al repositorio.
        /// </summary>
        /// <param name="entities">Las entidades a añadir.</param>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Marca una entidad existente como modificada en el repositorio.
        /// </summary>
        /// <param name="entity">La entidad a actualizar.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Marca una entidad existente para ser eliminada del repositorio.
        /// </summary>
        /// <param name="entity">La entidad a eliminar.</param>
        /// <returns>Devuelve true si la entidad fue encontrada y marcada para eliminar, false si no.</returns>
        bool Delete(TEntity entity);
        // Se eliminó DeleteAsync(TPrimaryKey id). Para eliminar por ID (simple o compuesto)
        // se debe usar una especificación para encontrarlo y luego llamar a DeleteAsync(entity),
        // o usar ExecuteDeleteAsync de IEfCoreRepository si está disponible.
    }
}
