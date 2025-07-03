using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Colección de extensiones para mejorar la seguridad al manipular cadenas sensibles
    /// </summary>
    public static class SecureStringExt
    {
        /// <summary>
        /// Convierte una cadena de texto en un objeto <see cref="SecureString"/>.
        /// </summary>
        /// <param name="value">La cadena de texto a convertir. Puede ser nula o vacía.</param>
        /// <returns>
        /// Un objeto <see cref="SecureString"/> que contiene los caracteres de la cadena de entrada,
        /// o <c>null</c> si la cadena de entrada es nula o vacía.
        /// </returns>
        /// <remarks>
        /// - El objeto <see cref="SecureString"/> resultante se marca como de solo lectura para mayor seguridad.
        /// - Este método es útil para manejar datos sensibles como contraseñas.
        /// </remarks>
        public static SecureString? ToSecureString(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var secureString = new SecureString();
            foreach (var c in value)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }
        /// <summary>
        /// Compara dos cadenas SecureString de manera segura.
        /// </summary>
        /// <param name="left">Primer SecureString a comparar.</param>
        /// <param name="right">Segundo SecureString a comparar.</param>
        /// <returns>true si los 2 SecureStrings son iguales; de lo contrario, false.</returns>
        /// <remarks>
        /// - La comparación es caso sensitivo y de culture-invariant (Ordinal).
        /// - No se permiten comparar con nulos; se lanzará <see cref="ArgumentNullException"/>.
        /// - Administra la memoria no administrada para prevenir la exposición de datos sensitivos en dicha memoria.
        /// - Nota importante de seguridad: Para realizar la comparación, este método convierte temporalmente el contenido de
        ///   ambos <see cref="SecureString"/> a cadenas <see cref="string"/> administradas en memoria. Aunque estas cadenas
        ///   son efímeras y el GC las recolectará eventualmente, su contenido existe brevemente en la memoria administrada.
        ///   Para escenarios de altísima seguridad donde esto no es aceptable, se requeriría una comparación directa
        ///   byte a byte en memoria no administrada.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Si <paramref name="left"/> o <paramref name="right"/> es nulo.
        /// </exception>
        public static bool SecureCompare(this SecureString left, SecureString right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left), "El SecureString izquierdo (left) no puede ser nulo.");
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right), "El SecureString derecho (right) no puede ser nulo.");
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            IntPtr leftPtr = IntPtr.Zero;
            IntPtr rightPtr = IntPtr.Zero;

            try
            {
                leftPtr = Marshal.SecureStringToGlobalAllocUnicode(left);
                rightPtr = Marshal.SecureStringToGlobalAllocUnicode(right);

                string leftString = Marshal.PtrToStringUni(leftPtr) ?? string.Empty;
                string rightString = Marshal.PtrToStringUni(rightPtr) ?? string.Empty;

                return leftString.Equals(rightString, StringComparison.Ordinal);
            }
            finally
            {
                if (leftPtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(leftPtr);
                }

                if (rightPtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(rightPtr);
                }
            }
        }

    }
}
