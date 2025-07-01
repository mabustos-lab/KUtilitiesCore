using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Servicio encargado de gestionar la obtención de releases desde un repositorio de GitHub
    /// para actualizar una aplicación. Permite filtrar releases por canal (QA, Producción, etc.)
    /// y extraer la versión de los tags de las releases.
    /// </summary>
    public class GitHubUpdateService
    {
        /// <summary>
        /// Información de configuración y autenticación para la actualización de la aplicación.
        /// </summary>
        private IAppUpdateInfo _info;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de actualización desde GitHub.
        /// </summary>
        /// <param name="info">Información de la aplicación y repositorio para la actualización.</param>
        public GitHubUpdateService(IAppUpdateInfo info) { _info = info; }

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

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Siomax");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "token",
                _info.GetDecryptedToken());
            GitHubRelease? result = null;
            string apiUrl = $"https://api.github.com/repos/{_info.GitHub.Owner}/{_info.GitHub.Repository}/releases";
            try
            {
                var response = await client.GetAsync(apiUrl).ConfigureAwait(true);
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                var releases = Helpers.Utilities.FromJson<List<GitHubRelease>>(body);

                result = releases
                    .Where(
                        r => r.TagName != null &&
                            r.TagName.IndexOf(_info.UpdateChannel, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderByDescending(r => Version.Parse(ExtractVersion(r.TagName ?? string.Empty)))
                    .FirstOrDefault();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Error de red al consultar GitHub: {ex}");
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"La solicitud a GitHub fue cancelada o expiró: {ex}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Acceso no autorizado a la API de GitHub: {ex}");
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"Error al analizar la versión de la release: {ex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado al obtener la release: {ex}");
            }
            return result;
        }

        /// <summary>
        /// Extrae la versión numérica de un tag de release de GitHub.
        /// Por ejemplo, convierte "v1.2.3-qa" en "1.2.3".
        /// </summary>
        /// <param name="tag">
        /// Tag de la release en formato <c>v#.#.#-Channel</c> donde # es un número y Channel es el canal (qa, dev, prod).
        /// </param>
        /// <returns>La versión extraída como cadena, sin el prefijo "v" ni el sufijo de canal.</returns>
        private static string ExtractVersion(string tag)
        {
            // Extrae la versión del tag, ej. "v1.2.3-qa" --> "1.2.3"
            var parts = tag.Split('-')[0];
            return parts.TrimStart('v', 'V');
        }
    }
}
