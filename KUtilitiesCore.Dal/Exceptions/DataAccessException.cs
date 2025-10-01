namespace KUtilitiesCore.Dal.Exceptions
{
    /// <summary>
    /// Representa errores que ocurren durante las operaciones de acceso a datos.
    /// </summary>
    public class DataAccessException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DataAccessException"/>.
        /// </summary>
        public DataAccessException()
        { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DataAccessException"/> con un
        /// mensaje de error especificado.
        /// </summary>
        /// <param name="message">El mensaje que describe el error.</param>
        public DataAccessException(string message) : base(message) { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DataAccessException"/> con un
        /// mensaje de error especificado y una referencia a la excepción interna que es la causa de
        /// esta excepción.
        /// </summary>
        /// <param name="message">El mensaje que describe el error.</param>
        /// <param name="innerException">La excepción que es la causa de la excepción actual.</param>
        public DataAccessException(string message, Exception innerException) : base(message, innerException) { }

        #endregion Constructors
    }
}