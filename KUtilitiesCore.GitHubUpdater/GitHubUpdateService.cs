using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Servicio encargado de gestionar la obtención de releases desde un repositorio de GitHub
    /// para actualizar una aplicación. Permite filtrar releases por canal (QA, Producción, etc.).
    /// </summary>
    public class GitHubUpdateService
    {
        /// <summary>
        /// Información de configuración y autenticación para la actualización de la aplicación.
        /// </summary>
        private readonly IAppUpdateInfo _info;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de actualización desde GitHub.
        /// </summary>
        /// <param name="info">Información de la aplicación y repositorio para la actualización.</param>
        public GitHubUpdateService(IAppUpdateInfo info) 
        { 
            _info = info ?? throw new ArgumentNullException(nameof(info)); 
        }

        /// <summary>
        /// Obtiene la última release disponible en el canal de actualización configurado.
        /// Realiza una consulta autenticada a la API de GitHub y filtra las releases por canal.
        /// </summary>
        /// <returns>
        /// La última <see cref="GitHubRelease"/> correspondiente al canal, o <c>null</c> si no hay releases.
        /// </returns>
        public async Task<GitHubRelease?> GetLatestReleaseAsync()
        {
#if NET48
            using var handler = new HttpClientHandler
            {
                UseProxy = false,
                Proxy = null
            };
            using var client = new HttpClient(handler);
#else
            using var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
            };
            using var client = new HttpClient(handler);
#endif

            client.DefaultRequestHeaders.UserAgent.ParseAdd("KUtilitiesCore-Updater");
            
            string token = _info.GetDecryptedToken();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
            }

            string apiUrl = $"{_info.GitHub.API_URL}/repos/{_info.GitHub.Owner}/{_info.GitHub.Repository}/releases";
            try
            {
                var response = await client.GetAsync(apiUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var releases = Helpers.Utilities.FromJson<List<GitHubRelease>>(body);

                if (releases == null || !releases.Any())
                    return null;

                // Filtrar por canal si está especificado
                IEnumerable<GitHubRelease> filteredReleases = releases.Where(r => r.TagName != null);

                if (!string.IsNullOrWhiteSpace(_info.UpdateChannel))
                {
                    filteredReleases = filteredReleases.Where(
                        r => r.TagName.IndexOf(_info.UpdateChannel, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // El ordenamiento por versión se delega ahora al Manager, aquí devolvemos la más reciente por fecha de publicación/creación
                return filteredReleases
                    .OrderByDescending(r => r.PublishedAt ?? r.CreatedAt)
                    .FirstOrDefault();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de red al consultar GitHub: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"La solicitud a GitHub fue cancelada o expiró: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado al obtener la release: {ex.Message}");
            }
            return null;
        }
    }
}
