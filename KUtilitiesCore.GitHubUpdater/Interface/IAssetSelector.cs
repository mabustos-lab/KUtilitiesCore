using System.Collections.Generic;
using KUtilitiesCore.GitHubUpdater.Helpers;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Define una interfaz para seleccionar un asset de una lista basado en un patrón.
    /// </summary>
    public interface IAssetSelector
    {
        /// <summary>
        /// Selecciona el primer asset que coincida con el patrón proporcionado.
        /// </summary>
        /// <param name="assets">Lista de assets de GitHub.</param>
        /// <param name="pattern">Patrón de búsqueda (ej. comodines).</param>
        /// <returns>El asset encontrado o null si no hay coincidencias.</returns>
        GitHubAsset? Select(IEnumerable<GitHubAsset> assets, string pattern);
    }
}
