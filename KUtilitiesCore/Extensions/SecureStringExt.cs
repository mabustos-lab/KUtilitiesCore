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
        

        #region Comparación

        /// <summary>
        /// Compara dos cadenas SecureString de manera segura.
        /// </summary>
        /// <param name="left">Primer SecureString a comparar.</param>
        /// <param name="right">Segundo SecureString a comparar.</param>
        /// <returns>true si los 2 SecureStrings son iguales; de lo contrario, false.</returns>
        /// <remarks>
        /// - La comparación es caso sensitivo y de culture-invariant.
        /// - no se permiten comparar con nulos.
        /// - Administra la memoria para prevenir la exposición de datos sensitivos.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Si algun parámetro es nulo.
        /// </exception>
        public static bool SecureCompare(this SecureString left, SecureString right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left), "The left SecureString cannot be null.");
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right), "The right SecureString cannot be null.");
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

                string leftString = Marshal.PtrToStringUni(leftPtr);
                string rightString = Marshal.PtrToStringUni(rightPtr);

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

        #endregion


    }
}
