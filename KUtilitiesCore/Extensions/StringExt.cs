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
        #region Fields

        private static readonly Regex InvalidFileNameCharsRegex = new Regex(
                    $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]+",
                    RegexOptions.Compiled);

        #endregion Fields

        #region Methods

        /// <summary>
        /// Expande una cadena de texto (interpretada como un identificador, ej. "MiClaseEjemplo" o "HTMLParser")
        /// insertando espacios antes de las letras mayúsculas para formar palabras legibles.
        /// Maneja acrónimos (secuencias de mayúsculas) correctamente, manteniéndolos juntos.
        /// También reemplaza guiones bajos con espacios.
        /// </summary>
        /// <param name="inputString">La cadena de entrada a expandir.</param>
        /// <returns>Una cadena con espacios insertados para mejorar la legibilidad, o una cadena vacía si la entrada es nula o vacía.</returns>
        /// <example>
        /// "MiClaseEjemplo" se convierte en "Mi Clase Ejemplo".
        /// "HTMLParser" se convierte en "HTML Parser".
        /// "UnTexto_Con_Guiones" se convierte en "Un Texto Con Guiones".
        /// </example>
        public static string ExpandToWords(this string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;

            var result = new StringBuilder(inputString.Length + inputString.Length / 2); // Estimación inicial de capacidad

            for (int i = 0; i < inputString.Length; i++)
            {
                char currentChar = inputString[i];

                if (currentChar == '_')
                {
                    result.Append(' ');
                    continue;
                }

                if (i > 0 && char.IsUpper(currentChar))
                {
                    char prevChar = inputString[i - 1];
                    // Condición para insertar espacio:
                    // 1. La anterior no es mayúscula (ej. 'a' seguida de 'B' -> "a B")
                    // 2. O la anterior es mayúscula, PERO la siguiente también es minúscula (ej. "ABc" -> "A Bc", para manejar acrónimos seguidos de una palabra capitalizada)
                    if (!char.IsUpper(prevChar) ||
                        (i + 1 < inputString.Length && char.IsLower(inputString[i + 1]) && char.IsUpper(prevChar))
                       )
                    {
                        result.Append(' ');
                    }
                }
                result.Append(currentChar);
            }
            // Reemplazar múltiples espacios (que podrían surgir por guiones bajos consecutivos o al inicio/final) por uno solo y quitar espacios al inicio/final.
            return Regex.Replace(result.ToString(), @"\s+", " ").Trim();
        }

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

        /// <summary>
        /// Elimina los caracteres no válidos de una cadena de texto para generar un nombre de
        /// archivo válido.
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
        /// Normaliza una cadena de texto eliminando los caracteres de marcas diacríticas (acentos)
        /// y convirtiéndola a minúsculas. Si se especifica, puede mantener la sensibilidad a mayúsculas.
        /// </summary>
        /// <param name="value">La cadena de texto a normalizar.</param>
        /// <param name="caseSensitive">
        /// Indica si se debe mantener la sensibilidad a mayúsculas/minúsculas original de la cadena.
        /// Si es <c>true</c> (valor predeterminado), se mantiene el caso original después de quitar los diacríticos.
        /// Si es <c>false</c>, la cadena resultante se convierte a minúsculas.
        /// </param>
        /// <returns>
        /// Una cadena de texto normalizada sin marcas diacríticas. El caso de la cadena resultante
        /// dependerá del parámetro <paramref name="caseSensitive"/>.
        /// </returns>
        public static string ToNormalized(this string value, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string normalized = value.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return caseSensitive ? stringBuilder.ToString() : stringBuilder.ToString().ToLowerInvariant();
        }

        #endregion Methods
    }
}