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
    class HMSFormatter : ICustomFormatter, IFormatProvider
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
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            try
            {
                return string.Format(new PluralFormatter(),
                                    TimeFormats.TryGetValue(format, out var formatString) ?
                                    formatString : "{0}",
                                    arg);
            }
            catch (Exception ex)
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
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        #endregion Methods
    }

    /// <summary>
    /// Formateador para valores numéricos con pluralización.
    /// </summary>
    class PluralFormatter : ICustomFormatter, IFormatProvider
    {
        #region Fields

        private const string FormatSpecifier = "P";

        #endregion Fields

        #region Methods

        /// <summary>
        /// Formatea un valor numérico con pluralización.
        /// </summary>
        /// <param name="format">Especificador de formato</param>
        /// <param name="arg">Valor numérico</param>
        /// <param name="formatProvider">Proveedor de formato</param>
        /// <returns>Valor formateado</returns>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return arg?.ToString() ?? string.Empty;

            if (!(arg is int number))
                return HandleNonNumericArgument(arg);

            try
            {
                var parts = SplitFormat(format);
                var (singular, plural) = GetPluralForms(parts);

                return number == 1
                    ? FormatInvariant($"{number} {singular}")
                    : FormatInvariant($"{number} {plural}");
            }
            catch (Exception ex)
            {
                return HandleError(format, arg, ex);
            }
        }

        /// <summary>
        /// Obtiene una instancia del proveedor de formato para el tipo especificado.
        /// </summary>
        /// <param name="formatType">Tipo del proveedor de formato</param>
        /// <returns>Proveedor de formato</returns>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        private string FormatInvariant(string format)
        {
            return string.Format(CultureInfo.InvariantCulture, format);
        }

        private (string singular, string plural) GetPluralForms(string[] parts)
        {
            if (parts.Length < 3)
                throw new FormatException("Formato inválido. Debe ser 'P:Singular:Plural'.");

            return (parts[1], parts[2]);
        }

        private string HandleError(string format, object arg, Exception ex)
        {
            // Implementar lógica de manejo de errores centralizado
            return arg?.ToString() ?? string.Empty;
        }

        private string HandleNonNumericArgument(object arg)
        {
            if (arg is null)
                return string.Empty;

            return arg.ToString();
        }

        private string[] SplitFormat(string format)
        {
            ValidateFormatStartsWith(format, FormatSpecifier);

            return format.Split(':');
        }

        private void ValidateFormatStartsWith(string format, string expectedPrefix)
        {
            if (!format.StartsWith(expectedPrefix))
                throw new ArgumentException("Formato inválido. Debe comenzar con 'P'.");
        }

        #endregion Methods
    }
}