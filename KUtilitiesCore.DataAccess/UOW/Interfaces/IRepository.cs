namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz genérica para un repositorio de datos completo (lectura/escritura). Ya no necesita TPrimaryKey.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad gestionada por el repositorio.</typeparam>
    public interface IRepository<TEntity> : IReadOnlyDbRepository<TEntity>
        where TEntity : class
    {
        #region Methods

        /// <summary>
        /// Añade una nueva entidad al repositorio de forma asíncrona.
        /// </summary>
        Task<TEntity> AddAsync(TEntity entity);

        /// <summary>
        /// Añade una colección de nuevas entidades al repositorio de forma asíncrona.
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        /// <summary>
        /// Marca una entidad existente para ser eliminada del repositorio. La entidad debe ser
        /// obtenida primero (ej. usando FindOneAsync).
        /// </summary>
        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// Marca una entidad existente como modificada en el repositorio.
        /// </summary>
        Task UpdateAsync(TEntity entity);

        #endregion Methods

        // Se eliminó DeleteAsync(TPrimaryKey id). Para eliminar por ID (simple o compuesto) se debe
        // usar una especificación para encontrarlo y luego llamar a DeleteAsync(entity), o usar
        // ExecuteDeleteAsync de IEfCoreRepository si está disponible.
    }
}