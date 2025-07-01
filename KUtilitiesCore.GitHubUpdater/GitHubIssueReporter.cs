using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Permite crear issues en un repositorio de GitHub utilizando la API REST v3.
    /// Utiliza la información de autenticación y configuración proporcionada por <see cref="IAppUpdateInfo"/>.
    /// Compatible con .NET Framework 4.8 y .NET 8, adaptando el handler HTTP según la plataforma.
    /// </summary>
    public class GitHubIssueReporter
    {
        /// <summary>
        /// Información de configuración y autenticación para la actualización de la aplicación.
        /// </summary>
        private IAppUpdateInfo _info;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="GitHubIssueReporter"/> con la información de actualización especificada.
        /// </summary>
        /// <param name="info">Instancia que contiene la configuración y credenciales necesarias para interactuar con GitHub.</param>
        public GitHubIssueReporter(IAppUpdateInfo info)
        {
            _info = info;
        }

        /// <summary>
        /// Crea un nuevo issue en el repositorio de GitHub configurado.
        /// </summary>
        /// <param name="title">Título del issue.</param>
        /// <param name="body">Descripción o cuerpo del issue.</param>
        /// <returns>Una tarea que representa la operación asincrónica.</returns>
        /// <exception cref="HttpRequestException">Se produce si la solicitud HTTP no es exitosa.</exception>
        public async Task CreateIssueAsync(string title, string body)
        {
#if NET48
            // Configuración específica para .NET Framework 4.8
            using var handler = new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null
            };
            using var client = new HttpClient(handler);
#else
            // Configuración óptima para .NET 8
            using var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
            };
            using var client = new HttpClient(handler);
#endif

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Siomax");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "token",
                _info.GetDecryptedToken());
            var payload = new
            {
                title = title,
                body = body,
                labels = _info.GitHub.IssueLabels
            };
            var content = new StringContent(Helpers.Utilities.ToJson(payload), System.Text.Encoding.UTF8, "application/json");
            var url = $"https://api.github.com/repos/{_info.GitHub.Owner}/{_info.GitHub.Repository}/issues";
            var response = await client.PostAsync(url, content).ConfigureAwait(true);
            response.EnsureSuccessStatusCode();
        }
    }
}
