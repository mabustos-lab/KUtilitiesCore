using System;
using System.Linq;

namespace KUtilitiesCore.Dal.SQLLog
{
    /// <summary>
    /// Configura el comportamiento del logging de mensajes SQL: niveles mínimos, filtros
    /// personalizados y límites de tamaño para los mensajes capturados.
    /// </summary>
    public class SqlLoggingOptions
    {
        /// <summary>Nivel mínimo de severidad que debe tener un mensaje para registrarse.</summary>
        public SqlLogLevel MinimumLogLevel { get; set; } = SqlLogLevel.Info;
        /// <summary>Indica si se captura la información de conexión (servidor, base de datos) de cada evento.</summary>
        public bool IncludeConnectionInfo { get; set; } = true;
        /// <summary>Indica si la entrada de log incluye la marca de tiempo de recepción.</summary>
        public bool IncludeTimestamp { get; set; } = true;
        /// <summary>Indica si se descartan los mensajes generados por el sistema.</summary>
        public bool FilterSystemMessages { get; set; } = true;
        /// <summary>Números de error de SQL Server que se ignoran al registrar.</summary>
        public int[] IgnoredErrorNumbers { get; set; } = Array.Empty<int>();
        /// <summary>
        /// Filtro personalizado que decide si una entrada se registra. Null para no aplicar
        /// ningún filtro adicional.
        /// </summary>
        public Func<SqlLogEntry, bool>? CustomFilter { get; set; }
        /// <summary>Longitud máxima del mensaje registrado; los mensajes más largos se truncan.</summary>
        public int MaxMessageLength { get; set; } = 4000;
    }
}
