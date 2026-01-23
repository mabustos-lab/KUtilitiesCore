namespace KUtilitiesCore.Dal.ConnectionBuilder
{
    /// <summary>
    /// Expone los métodos para persistencia del Connection Builder
    /// </summary>
    public interface IConnectionBuilderPersist
    {
        /// <summary>
        /// Carga los valores almacenados en el objeto.
        /// </summary>
        void Load();
        /// <summary>
        /// Almacena los cambios realizados en el objeto.
        /// </summary>
        void SaveChanges();
    }
}