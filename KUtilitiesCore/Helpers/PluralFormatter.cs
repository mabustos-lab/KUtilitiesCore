﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace KUtilitiesCore.Helpers
{
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
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                return arg?.ToString() ?? string.Empty;

            if (arg is not int number)
                return HandleNonNumericArgument(arg);

            try
            {
                var parts = SplitFormat(format??string.Empty);
                var (singular, plural) = GetPluralForms(parts);

                return number == 1
                    ? FormatInvariant($"{number} {singular}")
                    : FormatInvariant($"{number} {plural}");
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
               return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene una instancia del proveedor de formato para el tipo especificado.
        /// </summary>
        /// <param name="formatType">Tipo del proveedor de formato</param>
        /// <returns>Proveedor de formato</returns>
        public object? GetFormat(Type? formatType)
        {
            if (formatType == null)
                throw new ArgumentNullException("formatType");
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        private static string FormatInvariant(string format)
        {
            return string.Format(CultureInfo.InvariantCulture, format);
        }

        private static (string singular, string plural) GetPluralForms(string[] parts)
        {
            if (parts.Length < 3)
                throw new FormatException("Formato inválido. Debe ser 'P:Singular:Plural'.");

            return (parts[1], parts[2]);
        }

        private static string HandleNonNumericArgument(object? arg)
        {
            return arg?.ToString() ?? string.Empty;
        }

        private static string[] SplitFormat(string format)
        {
            ValidateFormatStartsWith(format, FormatSpecifier);

            return format.Split(':');
        }

        private static void ValidateFormatStartsWith(string format, string expectedPrefix)
        {
            if (!format.StartsWith(expectedPrefix))
                throw new ArgumentException("Formato inválido. Debe comenzar con 'P'.");
        }

        #endregion Methods
    }
}