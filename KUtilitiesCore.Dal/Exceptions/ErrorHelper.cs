using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.Exceptions
{
    /// <summary>
    /// Encapsula la lógica de traducción de ErrorCode a excepciones
    /// </summary>
    public class ErrorHelper
    {
        private readonly Dictionary<int, Func<string, Exception>> _map;

        private ErrorHelper(Dictionary<int, Func<string, Exception>> map)
        {
            _map = map;
        }

        /// <summary>
        /// Lanza la excepción correspondiente si el código es distinto de 0.
        /// </summary>
        public void ThrowIfError(int errorCode, string errorMessage)
        {
            if (errorCode == 0) return;

            if (_map.TryGetValue(errorCode, out var factory))
            {
                throw factory(errorMessage);
            }

            // Fallback genérico si no está registrado
            throw new InvalidOperationException($"ErrorCode {errorCode}: {errorMessage}");
        }

        /// <summary>
        /// Builder para crear instancias configuradas de ErrorHelper.
        /// </summary>
        public class Builder
        {
            private readonly Dictionary<int, Func<string, Exception>> _map =
                new Dictionary<int, Func<string, Exception>>();

            /// <summary>
            /// Registra un código de error con su excepción asociada.
            /// </summary>
            public Builder Register(int errorCode, Func<string, Exception> factory)
            {
                _map[errorCode] = factory;
                return this;
            }

            /// <summary>
            /// Registra un rango de códigos con la misma excepción.
            /// </summary>
            public Builder RegisterRange(int start, int end, Func<string, Exception> factory)
            {
                for (int code = start; code <= end; code++)
                    _map[code] = factory;
                return this;
            }

            /// <summary>
            /// Construye la instancia final de ErrorHelper.
            /// </summary>
            /// <example>
            /// <code>
            /// // Configuración inicial con los códigos estándar
            /// var errorHelper = new ErrorHelper.Builder()
            ///     .RegisterRange(50000, 50099, msg => new System.Data.DBConcurrencyException(msg)) // Concurrencia
            ///     .RegisterRange(50100, 50199, msg => new ArgumentException(msg))                // Parámetros
            ///     .RegisterRange(50200, 50299, msg => new InvalidOperationException(msg))        // Integridad
            ///     .RegisterRange(50300, 50399, msg => new FormatException(msg))                 // JSON
            ///     .RegisterRange(50400, 50499, msg => new UnauthorizedAccessException(msg))     // Seguridad
            ///     .RegisterRange(50500, 50599, msg => new ApplicationException(msg))            // Auditoría
            ///     .Build();
            /// </code>
            /// </example>
            public ErrorHelper Build() => new ErrorHelper(new Dictionary<int, Func<string, Exception>>(_map));
        }
    }

}
