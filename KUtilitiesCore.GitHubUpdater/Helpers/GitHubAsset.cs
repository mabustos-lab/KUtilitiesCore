using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    /// <summary>
    /// Representa un archivo adjunto (asset) asociado a una release de GitHub.
    /// </summary>
    public class GitHubAsset
    {
        /// <summary>
        /// Identificador único del asset en GitHub.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Nombre del archivo del asset.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Etiqueta descriptiva opcional del asset.
        /// </summary>
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Tipo de contenido MIME del asset.
        /// </summary>
        [JsonPropertyName("content_type")]
        [JsonProperty("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Tamaño del asset en bytes.
        /// </summary>
        [JsonPropertyName("size")]
        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>
        /// URL para descargar el asset directamente desde el navegador.
        /// </summary>
        [JsonPropertyName("browser_download_url")]
        [JsonProperty("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
