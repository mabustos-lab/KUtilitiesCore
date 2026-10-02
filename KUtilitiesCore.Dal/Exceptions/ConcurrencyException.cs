namespace KUtilitiesCore.Dal.Exceptions
{
    /// <summary>
    /// Excepción que se lanza cuando se detecta un conflicto de concurrencia, típicamente cuando
    /// la versión de los datos en memoria no coincide con la versión almacenada en la base de datos.
    /// </summary>
    public class ConcurrencyException : Exception
    {
        #region Constructors

        /// <summary>
        /// Inicializa la excepción con el mensaje predeterminado de conflicto de concurrencia.
        /// </summary>
        public ConcurrencyException() : base("Se detectó un conflicto de concurrencia...")
        {
        }

        /// <summary>
        /// Inicializa la excepción con el mensaje especificado.
        /// </summary>
        /// <param name="message">Mensaje que describe el conflicto de concurrencia.</param>
        public ConcurrencyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Inicializa la excepción con el mensaje especificado y la excepción interna que la originó.
        /// </summary>
        /// <param name="message">Mensaje que describe el conflicto de concurrencia.</param>
        /// <param name="innerException">Excepción interna que originó este conflicto.</param>
        public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion Constructors
    }
}