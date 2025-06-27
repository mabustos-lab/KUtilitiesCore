using System;
using System.Linq;

namespace KUtilitiesCore.GitHubUpdater.AssetDownloader
{
    /// <summary>
    /// Envia los datos del estado del evento
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Obtiene la excepcion producida durante la descarga de la actualzación.
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Establece el procentaje de avance de la descarga
        /// </summary>
        public int Progress { get; private set; }

        /// <summary>
        /// Establece el estado actual del proceso.
        /// </summary>
        public DownloadStatus Status { get; private set; }

        /// <summary>
        /// Crea una nueva instancia con la información del progreso.
        /// </summary>
        public static DownloadProgressEventArgs Create(
            DownloadStatus status,
            int downloadProgress = 0,
            Exception? exception = null) => new()
        {
            Status = status,
            Progress = downloadProgress,
            Exception = exception
        };
    }
}