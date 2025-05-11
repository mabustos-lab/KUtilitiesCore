using KUtilitiesCore.DataAccess.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.Helpers
{
    /// <summary>
    /// Encapsula el resultado de una prueba de conexión.
    /// </summary>
    public class TestConnectionResult
    {
        /// <summary>
        /// Excepción que ocurrió durante la prueba de conexión, si aplica.
        /// </summary>
        public DataDbException Ex { get; set; }

        /// <summary>
        /// Indica si la prueba de conexión fue exitosa.
        /// </summary>
        public bool Sucess { get; set; }

        /// <summary>
        /// Versión del servidor al que se intentó conectar.
        /// </summary>
        public string ServerVersion { get; internal set; }

        /// <summary>
        /// Nombre publicado del servidor al que se intentó conectar.
        /// </summary>
        public string PublishedServerName { get; internal set; }

        /// <summary>
        /// Establece la versión del servidor.
        /// </summary>
        /// <param name="serverVersion">La versión del servidor.</param>
        internal void SetServerVersion(string serverVersion)
        {
            ServerVersion = serverVersion;
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
        /// Crea un resultado de prueba de conexión fallida.
        /// </summary>
        /// <param name="e">La excepción que ocurrió durante la prueba.</param>
        /// <returns>Una instancia de <see cref="TestConnectionResult"/> indicando el fallo.</returns>
        public static TestConnectionResult FailTest(DataDbException e)
        {
            return new TestConnectionResult
            {
                Sucess = false,
                Ex = e
            };
        }

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
    }
}
