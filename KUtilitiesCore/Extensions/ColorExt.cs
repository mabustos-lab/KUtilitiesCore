using KUtilities.Extensions;
using KUtilitiesCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class ColorExt
    {
        #region Methods

        /// <summary>
        /// Convierte una estructura de color a una cadena HTML.
        /// </summary>
        /// <param name="Value">Color a convertir.</param>
        /// <returns>Cadena HTML que representa el color.</returns>
        public static string ColorToHTML(this Color Value)
        {
            return ColorTranslator.ToHtml(Value);
        }

        /// <summary>
        /// Obtiene el color de contraste para el color que se pasa por parámetro.
        /// </summary>
        /// <param name="BackGrd">Color que se desea contrastar.</param>
        /// <returns>Color de contraste (Blanco o Negro).</returns>
        public static Color GetContrastColor(this Color BackGrd)
        {
            // Calculando la luminancia perceptiva - el ojo humano favorece el color verde.
            double a = 1 - (0.299 * BackGrd.R + 0.587 * BackGrd.G + 0.114 * BackGrd.B) / 255;
            int d = a < 0.5 ? 0 : 255;
            return Color.FromArgb(d, d, d);
        }

        /// <summary>
        /// Genera la lista de todos los colores con nombres conocidos, incluidos los colores del sistema.
        /// </summary>
        /// <returns>Enumeración de pares clave-valor de colores conocidos y sus valores.</returns>
        public static IEnumerable<KeyValuePair<KnownColor, Color>> GetKnowAllColors()
        {
            IEnumerable<KnownColor> Names = Enum.GetValues(typeof(KnownColor)).Cast<KnownColor>();
            return Names.Select(c => new KeyValuePair<KnownColor, Color>(c, Color.FromKnownColor(c)));
        }

        /// <summary>
        /// Genera la lista de todos los colores con nombres conocidos, sin incluir los colores del sistema.
        /// </summary>
        /// <returns>Enumeración de pares clave-valor de colores conocidos y sus valores.</returns>
        public static IEnumerable<KeyValuePair<KnownColor, Color>> GetKnowColors()
        {
            Type cKnowColor = typeof(KnownColor);
            Type ctype = typeof(Color);
            IEnumerable<PropertyInfo> propInfos = ctype
                .GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Cast<PropertyInfo>();
            return propInfos.Select(c => new KeyValuePair<KnownColor, Color>((KnownColor)Enum.Parse(cKnowColor, c.Name), (Color)c.GetValue(null)));
        }

        /// <summary>
        /// Genera la lista de todos los colores con nombres en su respectivo idioma, dependiendo del sistema operativo.
        /// </summary>
        /// <param name="IncludeSystemColors">Indica si se deben incluir los colores del sistema.</param>
        /// <returns>Enumeración de pares clave-valor de nombres de colores localizables y sus valores.</returns>
        public static IEnumerable<KeyValuePair<string, Color>> GetKnowLocalizableColors(bool IncludeSystemColors = false)
        {
            KeyValuePair<string, Color> p(KeyValuePair<KnownColor, Color> a) => new KeyValuePair<string, Color>(
                ResourceHelpers.GetFromResource(typeof(ColorExtLocalizable), q => q.GetString(a.Key.ToString())),
                a.Value);
            Func<KeyValuePair<KnownColor, Color>, KeyValuePair<string, Color>> ConvertionHelper = p;
            if (IncludeSystemColors)
            {
                return GetKnowAllColors().Select(c => ConvertionHelper(c));
            }
            return GetKnowColors().Select(c => ConvertionHelper(c));
        }

        /// <summary>
        /// Convierte una representación de color HTML en una estructura de color GDI+.
        /// </summary>
        /// <param name="HtmlCode">Código HTML del color.</param>
        /// <returns>Color GDI+ correspondiente al código HTML.</returns>
        public static Color HtmlToColor(string HtmlCode)
        {
            return ColorTranslator.FromHtml(HtmlCode);
        }

        #endregion Methods
    }
}
