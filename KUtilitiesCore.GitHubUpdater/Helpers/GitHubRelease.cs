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
    /// Representa una versión (release) de un repositorio en GitHub.
    /// </summary>
    public class GitHubRelease
    {
        /// <summary>
        /// Identificador único de la release en GitHub.
        /// </summary>
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Nombre de la etiqueta (tag) asociada a la release.
        /// </summary>
        [JsonPropertyName("tag_name")]
        [JsonProperty("tag_name")]
        public string? TagName { get; set; }

        /// <summary>
        /// Nombre descriptivo de la release.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Descripción o notas de la release.
        /// </summary>
        [JsonPropertyName("body")]
        [JsonProperty("body")]
        public string? Body { get; set; }

        /// <summary>
        /// Indica si la release es un borrador (draft).
        /// </summary>
        [JsonPropertyName("draft")]
        [JsonProperty("draft")]
        public bool Draft { get; set; }

        /// <summary>
        /// Indica si la release es una pre-lanzamiento (prerelease).
        /// </summary>
        [JsonPropertyName("prerelease")]
        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la release.
        /// </summary>
        [JsonPropertyName("created_at")]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Fecha y hora de publicación de la release (puede ser nulo si no ha sido publicada).
        /// </summary>
        [JsonPropertyName("published_at")]
        [JsonProperty("published_at")]
        public DateTime? PublishedAt { get; set; }

        /// <summary>
        /// Lista de archivos adjuntos (assets) asociados a la release.
        /// </summary>
        [JsonPropertyName("assets")]
        [JsonProperty("assets")]
        public List<GitHubAsset>? Assets { get; set; }
    }
}
