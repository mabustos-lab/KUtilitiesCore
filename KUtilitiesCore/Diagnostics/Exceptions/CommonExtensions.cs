using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace KUtilitiesCore.Diagnostics.Exceptions
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
            if (method == null) { throw new ArgumentNullException(nameof(method)); }

            var indentWidth = 4;
            var indent = new Func<int, string>(depth => new string(' ', indentWidth * depth));

            var parameters = method.GetParameters().Select(p => $"{p.ParameterType.ToShortString()} {p.Name}");

            var accessModifier = new[]
            {
                method.IsPublic ? "public" : string.Empty,
                method.IsAssembly ? "internal" : string.Empty,
                method.IsPrivate ? "private" : string.Empty,
                method.IsFamily ? "protected" : string.Empty,
                "[Unknow]"
            }
            .First(x => !string.IsNullOrEmpty(x));

            var inheritanceModifier = new[]
            {
                method.IsAbstract ? " abstract" : string.Empty,
                method.IsVirtual ? " virtual" : string.Empty,
                method.GetBaseDefinition() != method ? " override" : string.Empty,
            }
            .FirstOrDefault(x => !string.IsNullOrEmpty(x));

            var signature = new StringBuilder()
                .Append(" { ")
                .Append(accessModifier)
                .Append(method.IsStatic ? " static" : string.Empty)
                .Append(inheritanceModifier)
                .Append(method.GetCustomAttribute<AsyncStateMachineAttribute>() != null ? " async" : string.Empty)
                .Append(" ").Append(method.ReturnType.ToShortString())
                .Append(" ").Append(method.Name)
                .Append("(").Append(string.Join(", ", parameters)).Append(") { ... }")
                .Append(" } ")
                .ToString();

            return signature;
        }

        #endregion Methods
    }
}