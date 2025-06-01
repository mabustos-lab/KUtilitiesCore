using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace KUtilitiesCore.Logger.Info
{
    public static class CommonExtensions
    {
        #region Methods

        /// <summary>
        /// Genera un ID de la excepción calculado el Hash del texto generado de todos los StackTrace
        /// </summary>
        public static string GetExceptionHash(this Exception ex)
        {
            // Combina los detalles relevantes de la excepción en un solo string
            string exceptionDetails = $"{ex.GetType().Name}:{ex.Message}:{ex.StackTrace}";

            // Usa SHA256 para crear el hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(exceptionDetails);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                // Convierte el hash en una representación hexadecimal
                StringBuilder hashString = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashString.Append(b.ToString("X2")); // "X2" para formato hexadecimal
                }

                return hashString.ToString();
            }
        }

        /// <summary>
        /// Convierte un tipo en su representación de cadena corta.
        /// </summary>
        /// <param name="type">El tipo a convertir.</param>
        /// <returns>Una cadena que representa el tipo en formato corto.</returns>
        public static string ToShortString(this Type type)
        {
            var typeSyntax = SyntaxFactory.ParseTypeName(type.FullName);
            return typeSyntax.ToString();
        }

        /// <summary>
        /// Convierte un MethodInfo en su representación de cadena corta.
        /// </summary>
        /// <param name="method">El MethodInfo a convertir.</param>
        /// <returns>Una cadena que representa el MethodInfo en formato corto.</returns>
        /// <exception cref="ArgumentNullException">
        /// Se lanza cuando el parámetro method es nulo.
        /// </exception>
        public static string ToShortString(this MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            var parameters = method.GetParameters()
                .Select(p => $"{p.ParameterType.ToShortString()} {p.Name}");

            string accessModifier =
                method.IsPublic ? "public" :
                method.IsFamily ? "protected" :
                method.IsAssembly ? "internal" :
                method.IsPrivate ? "private" :
                "[Unknown]";

            string inheritanceModifier =
                method.IsAbstract ? " abstract" :
                method.GetBaseDefinition() != method ? " override" :
                method.IsVirtual ? " virtual" :
                string.Empty;

            bool isAsync = method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;

            var signature = new StringBuilder()
                .Append("{ ")
                .Append(accessModifier)
                .Append(method.IsStatic ? " static" : string.Empty)
                .Append(inheritanceModifier)
                .Append(isAsync ? " async" : string.Empty)
                .Append(" ").Append(method.ReturnType.ToShortString())
                .Append(" ").Append(method.Name)
                .Append("(").Append(string.Join(", ", parameters)).Append(") { ... }")
                .Append(" }")
                .ToString();

            return signature;
        }

        #endregion Methods
    }
}