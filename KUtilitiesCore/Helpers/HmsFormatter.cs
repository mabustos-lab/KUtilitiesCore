using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Helpers
{
    /// <summary>
    /// Formateador de tiempo en segundos, minutos, horas o días.
    /// </summary>
    class HmsFormatter : ICustomFormatter, IFormatProvider
    {
        #region Fields

        private static readonly Dictionary<string, string> TimeFormats = new Dictionary<string, string>
    {
        {"S", "{0:P:s:s}"},
        {"M", "{0:P:M:M}"},
        {"H", "{0:P:H:H}"},
        {"D", "{0:P:D:D}"}
    };

        #endregion Fields

        #region Methods

        /// <summary>
        /// Formatea un valor según el formato especificado.
        /// </summary>
        /// <param name="format">Especificador de formato</param>
        /// <param name="arg">Valor a formatear</param>
        /// <param name="formatProvider">Proveedor de formato</param>
        /// <returns>Valor formateado</returns>
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            try
            {
                return string.Format(new PluralFormatter(),
                                    TimeFormats.TryGetValue(format??string.Empty, out var formatString) ?
                                    formatString : "{0}",
                                    arg);
            }
            catch (Exception)
            {
                // Log del error si es necesario
                return arg?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Obtiene una instancia del proveedor de formato para el tipo especificado.
        /// </summary>
        /// <param name="formatType">Tipo del proveedor de formato</param>
        /// <returns>Proveedor de formato</returns>
        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        #endregion Methods
    }
}