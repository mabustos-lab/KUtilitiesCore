using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Linq;
using System.Security;


namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Configuración del repositorio de GitHub del que el actualizador obtiene las releases:
    /// propietario, nombre, URL de la API, etiquetas de issues y token de autenticación cifrado.
    /// </summary>
    public sealed class GitHubRepositoryInfo : IGitHubRepositoryInfo
    {
        /// <summary>
        /// Inicializa una instancia vacía con el token cifrado en cadena vacía,
        /// apta para rellenarse mediante enlace de configuración.
        /// </summary>
        public GitHubRepositoryInfo()
        {
            EncryptedToken = string.Empty;
        }

        /// <summary>
        /// Token de autenticación de GitHub almacenado cifrado (DPAPI),
        /// de modo que nunca resida en claro en configuración.
        /// </summary>
        public string EncryptedToken { get; internal set; }

        /// <summary>
        /// Etiquetas que se aplican a los issues creados desde el cliente
        /// para distinguirlos del resto de incidencias del repositorio.
        /// </summary>
        public string[] IssueLabels { get; set; } = ["bug", "reported-from-client"];

        /// <summary>
        /// Propietario (usuario u organización) del repositorio de GitHub.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del repositorio de GitHub del que se consultan las releases.
        /// </summary>
        public string Repository { get; set; } = string.Empty;

        /// <summary>
        /// URL base de la API de GitHub; se puede sobrescribir para apuntar
        /// a una instancia de GitHub Enterprise.
        /// </summary>
        public string API_URL { get; set; } = "https://api.github.com";

      
    }
}