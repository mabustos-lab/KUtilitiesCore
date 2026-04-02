using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KUtilitiesCore.GitHubUpdater.Helpers;
using KUtilitiesCore.GitHubUpdater.Interface;

namespace KUtilitiesCore.GitHubUpdater.Helpers
{
    /// <summary>
    /// Selecciona assets basado en patrones de comodines.
    /// </summary>
    public class WildcardAssetSelector : IAssetSelector
    {
        /// <summary>
        /// Selecciona el primer asset que coincida con el patrón de comodines proporcionado.
        /// </summary>
        /// <param name="assets">Lista de assets de GitHub.</param>
        /// <param name="pattern">Patrón de búsqueda con comodines (ej. *.exe, *Setup*).</param>
        /// <returns>El asset encontrado o null si no hay coincidencias.</returns>
        public GitHubAsset? Select(IEnumerable<GitHubAsset> assets, string pattern)
        {
            if (assets == null || string.IsNullOrEmpty(pattern))
                return null;

            // Convert wildcard to regex:
            // 1. Escape special regex characters
            // 2. Replace escaped wildcard (*) with (.*)
            // 3. Replace escaped wildcard (?) with (.)
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            return assets.FirstOrDefault(a => a.Name != null && regex.IsMatch(a.Name));
        }
    }
}
