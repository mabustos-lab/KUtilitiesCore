using KUtilitiesCore.Dal.Paging;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz base para repositorios de solo lectura. Define métodos para consultar entidades sin modificarlas.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad.</typeparam>
    public interface IReadOnlyDbRepository<TEntity> where TEntity : class
    {
        #region Methods

        /// <summary>
        /// Cuenta el número de entidades que cumplen con la especificación proporcionada.
        /// </summary>
        /// <param name="specification">La especificación que define el criterio.</param>
        /// <returns>El número de entidades que cumplen la condición.</returns>
        Task<int> CountAsync(ISpecification<TEntity> specification = null);

        /// <summary>
        /// Comprueba si existe alguna entidad que cumpla con la especificación proporcionada.
        /// </summary>
        /// <param name="specification">La especificación que define el criterio.</param>
        /// <returns>True si existe al menos una entidad, false en caso contrario.</returns>
        Task<bool> ExistsAsync(ISpecification<TEntity> specification = null);

        /// <summary>
        /// Busca una lista de entidades que cumplan con la especificación proporcionada de forma
        /// asíncrona. La paginación no se aplica aquí; usar GetPagedAsync para resultados paginados.
        /// </summary>
        /// <param name="specification">
        /// La especificación que define los criterios de búsqueda, inclusiones y ordenación.
        /// </param>
        /// <returns>Una lista de solo lectura de entidades que cumplen la especificación.</returns>
        Task<IReadOnlyList<TEntity>> FindAsync(ISpecification<TEntity> specification = null);

        /// <summary>
        /// Busca una única entidad que cumpla con la especificación proporcionada de forma asíncrona.
        /// </summary>
        /// <param name="specification">
        /// La especificación que define los criterios de búsqueda, inclusiones y ordenación.
        /// </param>
        /// <returns>
        /// La primera entidad que cumple la especificación, o null si no se encuentra ninguna.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Si más de una entidad cumple la especificación y se esperaba una sola.
        /// </exception>
        Task<TEntity> FindOneAsync(ISpecification<TEntity> specification = null);

        /// <summary>
        /// Obtiene una página de resultados de entidades de forma asíncrona, basada en una
        /// especificación y opciones de paginación.
        /// </summary>
        /// <param name="pagingOptions">
        /// Las opciones para la paginación (número de página, tamaño y si omitir paginación).
        /// </param>
        /// <param name="specification">
        /// La especificación que define los criterios, inclusiones y ordenación.
        /// </param>
        /// <returns>Un objeto IPagedResult con los datos de la página solicitada.</returns>
        Task<IPagedResult<TEntity>> GetPagedAsync(IPagingOptions pagingOptions, ISpecification<TEntity> specification = null);

        #endregion Methods
    }
}