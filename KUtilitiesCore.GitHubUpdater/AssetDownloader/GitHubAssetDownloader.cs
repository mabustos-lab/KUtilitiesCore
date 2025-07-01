using KUtilitiesCore.Encryption.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security;

namespace KUtilitiesCore.GitHubUpdater.AssetDownloader
{
    /// <summary>
    /// Proporciona funcionalidad para descargar assets de GitHub utilizando un token de autenticación seguro.
    /// Reporta el progreso y el estado de la descarga mediante eventos.
    /// Compatible con .NET Framework 4.8 y .NET 8.
    /// </summary>
    public class GitHubAssetDownloader
    {
        /// <summary>
        /// Token de autenticación de GitHub almacenado de forma segura.
        /// </summary>
        private readonly SecureString secureGithubToken;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="GitHubAssetDownloader"/> con el token de GitHub proporcionado.
        /// </summary>
        /// <param name="githubToken">Token de autenticación de GitHub.</param>
        public GitHubAssetDownloader(SecureString githubToken) 
        { secureGithubToken = githubToken ?? throw new ArgumentNullException(nameof(githubToken)); }

        /// <summary>
        /// Evento que se dispara para reportar el progreso y estado de la descarga.
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        /// <summary>
        /// Invoca el evento <see cref="DownloadProgress"/> con el progreso, estado y excepción (si existe).
        /// </summary>
        /// <param name="progress">Porcentaje de progreso (0-100).</param>
        /// <param name="status">Estado de la descarga.</param>
        /// <param name="ex">Excepción ocurrida, si aplica.</param>
        private void OnProgresoDescarga(int progress, DownloadStatus status, Exception? ex)
        {
            DownloadProgress?.Invoke(
            this,
            DownloadProgressEventArgs
                .Create(status, progress, ex));
        }

#if NET8_0_OR_GREATER
            /// <summary>
            /// Descarga un archivo desde la URL especificada y lo guarda en la ruta de destino.
            /// Utiliza HttpClient y soporta cancelación y reporte de progreso.
            /// </summary>
            /// <param name="url">URL del asset a descargar.</param>
            /// <param name="destinationPath">Ruta donde se guardará el archivo descargado.</param>
            /// <param name="cancellationToken">Token de cancelación opcional.</param>
            public async Task DownloadAsync(
                string url,
                string destinationPath,
                CancellationToken cancellationToken = default)
            {
                try
                {
                     OnProgresoDescarga(0, DownloadStatus.Wait, null);

                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", secureGithubToken.ToPlainText());

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var total = response.Content.Headers.ContentLength ?? -1L;
                var canReport = total != -1;

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                int lastPercentage = 0;

                OnProgresoDescarga(0, DownloadStatus.Downloading, null);

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    totalRead += read;

                    if (canReport)
                    {
                        int percent = (int)((totalRead * 100L) / total);
                        if (percent != lastPercentage)
                        {
                            lastPercentage = percent;
                            OnProgresoDescarga(percent, DownloadStatus.Downloading, null);
                        }
                    }
                }

                OnProgresoDescarga(100, DownloadStatus.Downloaded, null);
                } catch(Exception ex)
                {
                    OnProgresoDescarga(0, DownloadStatus.Failed, ex);
                }
            }
#else
        /// <summary>
        /// Descarga un archivo desde la URL especificada y lo guarda en la ruta de destino.
        /// Utiliza WebClient y soporta cancelación y reporte de progreso.
        /// </summary>
        /// <param name="url">URL del asset a descargar.</param>
        /// <param name="destinationPath">Ruta donde se guardará el archivo descargado.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        public async Task DownloadAsync(
            string url,
            string destinationPath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                OnProgresoDescarga(0, DownloadStatus.Wait, null);

                using (var client = new WebClient())
                {
                    // Agregar User-Agent y Authorization
                    client.Headers.Add(HttpRequestHeader.UserAgent, "KUtilitiesCore");
                    client.Headers.Add(HttpRequestHeader.Authorization, "token " + secureGithubToken.ToPlainText());

                    // Progreso
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        OnProgresoDescarga(e.ProgressPercentage, DownloadStatus.Downloading, null);
                    };

                    // Cancelación
                    using (cancellationToken.Register(() => client.CancelAsync()))
                    {
                        OnProgresoDescarga(0, DownloadStatus.Downloading, null);

                        await client.DownloadFileTaskAsync(new Uri(url), destinationPath);

                        OnProgresoDescarga(100, DownloadStatus.Downloaded, null);
                    }
                }
            }
            catch (Exception ex)
            {
                OnProgresoDescarga(0, DownloadStatus.Failed, ex);
            }
        }
#endif
    }
}