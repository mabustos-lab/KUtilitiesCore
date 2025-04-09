using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para trabajar con cadenas de texto.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Expande una cadena de texto como el nombre de un campo, insertando un espacio en cada
        /// letra capitalizada, excepto cuando las letras capitalizadas son consecutivas (posible acrónimo).
        /// </summary>
        public static string ExpandToWords(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;

            var result = new StringBuilder(inputString.Length * 2); // Pre-asignar más espacio
            bool previousWasUpper = char.IsUpper(inputString[0]);
            result.Append(inputString[0]);

            for (int i = 1; i < inputString.Length; i++)
            {
                bool currentIsUpper = char.IsUpper(inputString[i]);
                if (currentIsUpper && !previousWasUpper)
                {
                    result.Append(' ');
                }
                result.Append(inputString[i]);
                previousWasUpper = currentIsUpper;
            }

            return result.Replace("_", " ").ToString().Trim();
        }

        /// <summary>
        /// Formatea una cadena de texto utilizando los argumentos proporcionados.
        /// </summary>
        public static string Format(this string format, params object[] args)
            => string.Format(format, args);

        /// <summary>
        /// Decodifica una cadena de texto codificada en Base64.
        /// </summary>
        public static string FromBase64String(this string source)
            => Encoding.UTF8.GetString(Convert.FromBase64String(source));

        /// <summary>
        /// Verifica si una cadena de texto está contenida dentro de un arreglo de cadenas.
        /// </summary>
        public static bool In(this string value, bool caseSensitive, params string[] stringValues)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return stringValues.Any(s => s.Equals(value, comparison));
        }

        /// <summary>
        /// Determina si dos rangos se intersectan.
        /// </summary>
        public static bool Intersect<T>(T x1, T y1, T x2, T y2) where T : IComparable<T>
            => x2.CompareTo(y1) <= 0 && x1.CompareTo(y2) <= 0;

        private static readonly Regex InvalidFileNameCharsRegex = new Regex(
            $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+",
            RegexOptions.Compiled);

        /// <summary>
        /// Elimina los caracteres no válidos de una cadena de texto para generar un nombre de archivo válido.
        /// </summary>
        public static string MakeValidFileName(this string fileName)
            => InvalidFileNameCharsRegex.Replace(fileName, "_");

        /// <summary>
        /// Codifica una cadena de texto en Base64.
        /// </summary>
        public static string ToBase64String(this string source)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(source));

        /// <summary>
        /// Convierte una cadena de texto en un valor enumerado del tipo especificado.
        /// </summary>
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, Enum
            => (TEnum)Enum.Parse(typeof(TEnum), value);

        /// <summary>
        /// Compara dos cadenas de texto normalizándolas e ignorando mayúsculas, minúsculas y acentos.
        /// </summary>
        public static bool IsEqual(this string string1, string string2)
        {
            if (ReferenceEquals(string1, string2)) return true;
            if (string1 is null || string2 is null) return false;

            var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return compareInfo.Compare(
                string1.Trim().Normalize(NormalizationForm.FormD),
                string2.Trim().Normalize(NormalizationForm.FormD),
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
        }
    }
}
