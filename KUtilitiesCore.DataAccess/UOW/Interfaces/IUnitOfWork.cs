namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Contrato para el patrón Unit of Work.
    /// Encargado de mantener una lista de repositorios y coordinar la escritura de cambios.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Obtiene una instancia del repositorio genérico para la entidad especificada.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <returns>Instancia de IRepository con soporte para Specifications.</returns>
        IRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Guarda todos los cambios realizados en el contexto actual de manera asíncrona.
        /// <para>
        /// Nota: En implementaciones de Base de Datos (EF/SQL), esto confirma la transacción (Commit).
        /// En implementaciones de API, puede simplemente validar el estado.
        /// </para>
        /// </summary>
        /// <returns>Número de registros afectados o indicador de éxito.</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Guarda todos los cambios realizados de manera síncrona.
        /// <para>
        /// Nota: En implementaciones de Base de Datos (EF/SQL), esto confirma la transacción (Commit).
        /// </para>
        /// </summary>
        /// <returns>Número de registros afectados o indicador de éxito.</returns>
        int SaveChanges();
    }
}
