using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para operaciones de seguridad y encriptación sobre cadenas de texto.
    /// </summary>
    public static class StringExt
    {
        /// <summary>
        /// Convierte una cadena de texto plano en una instancia de <see cref="ToSecureString"/>.
        /// </summary>
        /// <param name="plainText">La cadena de texto plano a convertir. No puede ser <see langword="null"/>.</param>
        /// <returns>
        /// Una instancia de <see cref="ToSecureString"/> que contiene los caracteres de la cadena de entrada.
        /// La instancia devuelta se marca como de solo lectura.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce si <paramref name="plainText"/> es <see langword="null"/>.
        /// </exception>
        public static SecureString ToSecureString(this string plainText)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            var secure = new SecureString();
            foreach (char c in plainText)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>
        /// Convierte un <see cref="SecureString"/> en texto cifrado utilizando el servicio de encriptación proporcionado.
        /// </summary>
        /// <param name="secure">El <see cref="ToSecureString"/> a convertir y cifrar. No puede ser <see langword="null"/>.</param>
        /// <param name="encryptionService">El servicio de encriptación a utilizar. No puede ser <see langword="null"/>.</param>
        /// <returns>El texto cifrado resultante de la conversión y encriptación del <see cref="ToSecureString"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce si <paramref name="secure"/> o <paramref name="encryptionService"/> es <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce si no se puede convertir el <see cref="ToSecureString"/> a texto plano.
        /// </exception>
        public static string ToEncryptedText(this SecureString secure, IEncryptionService encryptionService)
        {
            if (secure == null)
                throw new ArgumentNullException(nameof(secure));
            if (encryptionService == null)
                throw new ArgumentNullException(nameof(encryptionService));

            return encryptionService.Encrypt(secure.ToPlainText());
        }
        /// <summary>
        /// Convierte un <see cref="SecureString"/> en texto plano.
        /// </summary>
        /// <param name="secure">El <see cref="ToSecureString"/> a convertir y cifrar. No puede ser <see langword="null"/>.</param>
        /// <returns>El texto cifrado resultante de la conversión y encriptación del <see cref="ToSecureString"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se produce si <paramref name="secure"/> es <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Se produce si no se puede convertir el <see cref="ToSecureString"/> a texto plano.
        /// </exception>
        /// <remarks>Usar con cuidado para no exponer información sensible.</remarks>
        public static string ToPlainText(this SecureString secure)
        {
            if (secure == null)
                throw new ArgumentNullException(nameof(secure));
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secure);
                string plainText = System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString)!;
                if (plainText == null)
                    throw new InvalidOperationException("No se pudo convertir SecureString a texto plano.");
                return plainText;
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
