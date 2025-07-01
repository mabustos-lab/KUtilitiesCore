using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater.AssetDownloader
{
    /// <summary>
    /// Establece el estado de la descarga de la actualización.
    /// </summary>
    public enum DownloadStatus
    {
        /// <summary>
        /// Indica que esta en espera de inicializar la descarga.
        /// </summary>
        Wait,
        /// <summary>
        /// Indica que esta Descargando la actualización.
        /// </summary>
        Downloading,
        /// <summary>
        /// Indica que finalizo la descarga sin fallas
        /// </summary>
        Downloaded,
        /// <summary>
        /// Indica que el proceso falló en la descarga.
        /// </summary>
        Failed
    }
}
