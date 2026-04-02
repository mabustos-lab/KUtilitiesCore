using System;
using System.Threading;
using System.Threading.Tasks;
using KUtilitiesCore.GitHubUpdater.AssetDownloader;
using KUtilitiesCore.GitHubUpdater.Helpers;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Define los métodos para un administrador de actualizaciones de GitHub.
    /// Coordina la búsqueda de releases y la descarga de assets.
    /// </summary>
    public interface IGitHubUpdateManager
    {
        /// <summary>
        /// Evento que se dispara para reportar el progreso y estado de la descarga.
        /// </summary>
        event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        /// <summary>
        /// Busca la última release disponible en GitHub y verifica si es una versión superior a la actual.
        /// </summary>
        /// <returns>La última release disponible si es superior a la actual; de lo contrario, <c>null</c>.</returns>
        Task<GitHubRelease?> CheckForUpdatesAsync();

        /// <summary>
        /// Descarga un asset específico de una release de GitHub en la ruta de destino.
        /// Selecciona el asset basándose en el patrón configurado.
        /// </summary>
        /// <param name="release">La release de GitHub de la cual descargar el asset.</param>
        /// <param name="destinationPath">Ruta local donde se guardará el archivo descargado.</param>
        /// <param name="ct">Token de cancelación opcional.</param>
        /// <returns>Una tarea que representa la operación de descarga asincrónica.</returns>
        Task DownloadUpdateAsync(GitHubRelease release, string destinationPath, CancellationToken ct = default);
    }
}
