using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KUtilitiesCore.GitHubUpdater.AssetDownloader;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Orquestador para la gestión de actualizaciones desde GitHub.
    /// Coordina la búsqueda de nuevas versiones, la comparación de versiones y la descarga de assets.
    /// Implementa los principios SOLID al depender de abstracciones.
    /// </summary>
    public class GitHubUpdateManager : IGitHubUpdateManager
    {
        private readonly IAppUpdateInfo _appUpdateInfo;
        private readonly GitHubUpdateService _updateService;
        private readonly GitHubAssetDownloader _assetDownloader;
        private readonly IVersionParser _versionParser;
        private readonly IAssetSelector _assetSelector;

        /// <summary>
        /// Evento que se dispara para reportar el progreso y estado de la descarga de la actualización.
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        /// <summary>
        /// Inicializa una nueva instancia del administrador de actualizaciones de GitHub.
        /// </summary>
        /// <param name="appUpdateInfo">Información de la aplicación y configuración de actualización.</param>
        /// <param name="updateService">Servicio para interactuar con la API de GitHub para releases.</param>
        /// <param name="assetDownloader">Componente encargado de la descarga física de assets.</param>
        /// <param name="versionParser">Componente para extraer y comparar versiones desde tags de GitHub.</param>
        /// <param name="assetSelector">Estrategia para seleccionar el asset correcto basándose en patrones.</param>
        public GitHubUpdateManager(
            IAppUpdateInfo appUpdateInfo,
            GitHubUpdateService updateService,
            GitHubAssetDownloader assetDownloader,
            IVersionParser versionParser,
            IAssetSelector assetSelector)
        {
            _appUpdateInfo = appUpdateInfo ?? throw new ArgumentNullException(nameof(appUpdateInfo));
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
            _assetDownloader = assetDownloader ?? throw new ArgumentNullException(nameof(assetDownloader));
            _versionParser = versionParser ?? throw new ArgumentNullException(nameof(versionParser));
            _assetSelector = assetSelector ?? throw new ArgumentNullException(nameof(assetSelector));

            // Reenviar eventos de progreso del descargador
            _assetDownloader.DownloadProgress += (s, e) => DownloadProgress?.Invoke(this, e);
        }

        /// <summary>
        /// Verifica si existe una nueva actualización disponible en GitHub.
        /// </summary>
        /// <returns>La última release si es superior a la versión actual configurada; de lo contrario, <c>null</c>.</returns>
        public async Task<GitHubRelease?> CheckForUpdatesAsync()
        {
            var latestRelease = await _updateService.GetLatestReleaseAsync().ConfigureAwait(false);
            if (latestRelease == null || string.IsNullOrEmpty(latestRelease.TagName))
            {
                return null;
            }

            Version currentVersion = _versionParser.Parse(_appUpdateInfo.AppVersion);
            Version latestVersion = _versionParser.Parse(latestRelease.TagName);

            if (latestVersion > currentVersion)
            {
                return latestRelease;
            }

            return null;
        }

        /// <summary>
        /// Descarga el asset adecuado de la release especificada.
        /// </summary>
        /// <param name="release">La release que contiene los assets.</param>
        /// <param name="destinationPath">Ruta donde se guardará el archivo descargado.</param>
        /// <param name="ct">Token para cancelar la descarga.</param>
        /// <exception cref="InvalidOperationException">Si no se encuentra un asset que coincida con el patrón.</exception>
        public async Task DownloadUpdateAsync(GitHubRelease release, string destinationPath, CancellationToken ct = default)
        {
            if (release.Assets == null || !release.Assets.Any())
            {
                throw new InvalidOperationException("La release no contiene ningún asset para descargar.");
            }

            GitHubAsset? asset = _assetSelector.Select(release.Assets, _appUpdateInfo.AssetPattern);

            if (asset == null || string.IsNullOrEmpty(asset.BrowserDownloadUrl))
            {
                throw new InvalidOperationException($"No se pudo encontrar un asset que coincida con el patrón: '{_appUpdateInfo.AssetPattern}'.");
            }

            await _assetDownloader.DownloadAsync(asset.BrowserDownloadUrl, destinationPath, ct).ConfigureAwait(false);
        }
    }
}
