namespace KUtilitiesCore.Dal.Exceptions
{
    /// <summary>
    /// Excepción personalizada para errores de acceso a datos.
    /// </summary>
    public class DataAccessException : Exception
    {
        /// <summary>
        /// Obtiene la sentencia SQL que causó la excepción.
        /// </summary>
        public string Sql { get; }

        /// <summary>
        /// Obtiene el nombre del proveedor de base de datos.
        /// </summary>
        public string ProviderName { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DataAccessException"/>.
        /// </summary>
        /// <param name="message">Mensaje que describe el error.</param>
        /// <param name="sql">Sentencia SQL relacionada con el error.</param>
        /// <param name="providerName">Nombre del proveedor de base de datos.</param>
        /// <param name="innerException">Excepción interna que causó este error.</param>
        public DataAccessException(string message, string sql, string providerName, Exception innerException)
            : base(message, innerException)
        {
            Sql = sql;
            ProviderName = providerName;
        }
    }
}