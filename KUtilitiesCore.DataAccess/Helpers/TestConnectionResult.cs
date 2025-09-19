using KUtilitiesCore.DataAccess.Exceptions;

namespace KUtilitiesCore.DataAccess.Helpers
{
    /// <summary>
    /// Encapsula el resultado de una prueba de conexión.
    /// </summary>
    public class TestConnectionResult
    {
        #region Properties

        /// <summary>
        /// Obtiene un resultado de prueba de conexión exitosa.
        /// </summary>
        public static TestConnectionResult SucessTest
        {
            get
            {
                return new TestConnectionResult
                { Sucess = true };
            }
        }

        /// <summary>
        /// Excepción que ocurrió durante la prueba de conexión, si aplica.
        /// </summary>
        public DataAccessException Ex { get; set; }

        /// <summary>
        /// Nombre publicado del servidor al que se intentó conectar.
        /// </summary>
        public string PublishedServerName { get; internal set; }

        /// <summary>
        /// Versión del servidor al que se intentó conectar.
        /// </summary>
        public string ServerVersion { get; internal set; }

        /// <summary>
        /// Indica si la prueba de conexión fue exitosa.
        /// </summary>
        public bool Sucess { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Crea un resultado de prueba de conexión fallida.
        /// </summary>
        /// <param name="e">La excepción que ocurrió durante la prueba.</param>
        /// <returns>Una instancia de <see cref="TestConnectionResult"/> indicando el fallo.</returns>
        public static TestConnectionResult FailTest(DataAccessException e)
        {
            return new TestConnectionResult
            {
                Sucess = false,
                Ex = e
            };
        }

        /// <summary>
        /// Establece el nombre publicado del servidor.
        /// </summary>
        /// <param name="publishedServerName">El nombre publicado del servidor.</param>
        internal void SetPublishedServerName(string publishedServerName)
        {
            PublishedServerName = publishedServerName;
        }

        /// <summary>
        /// Establece la versión del servidor.
        /// </summary>
        /// <param name="serverVersion">La versión del servidor.</param>
        internal void SetServerVersion(string serverVersion)
        {
            ServerVersion = serverVersion;
        }

        #endregion Methods
    }
}