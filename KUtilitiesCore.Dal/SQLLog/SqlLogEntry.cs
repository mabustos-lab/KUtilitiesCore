using System;
using System.Linq;

namespace KUtilitiesCore.Dal.SQLLog
{
    /// <summary>
    /// Representa un mensaje capturado del servidor SQL (PRINT, RAISERROR o eventos de conexión)
    /// durante el logging SQL, con su contexto de origen y severidad.
    /// </summary>
    public class SqlLogEntry
    {
        /// <summary>Marca de tiempo UTC en la que se recibió el mensaje.</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>Nivel de severidad del mensaje recibido.</summary>
        public SqlLogLevel LogLevel { get; set; }
        /// <summary>Texto del mensaje recibido. Vacío si el servidor no lo proporcionó.</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>Procedimiento almacenado de origen del mensaje, si aplica.</summary>
        public string Procedure { get; set; } = string.Empty;
        /// <summary>Número de línea dentro del procedimiento de origen, si aplica.</summary>
        public int LineNumber { get; set; }
        /// <summary>Número de error de SQL Server, si aplica.</summary>
        public int ErrorNumber { get; set; }
        /// <summary>Severidad del error reportada por SQL Server.</summary>
        public int Severity { get; set; }
        /// <summary>Estado interno del error reportado por SQL Server.</summary>
        public int State { get; set; }
        /// <summary>Nombre del servidor de origen, si se incluyó en el evento.</summary>
        public string Server { get; set; } = string.Empty;
        /// <summary>Nombre de la base de datos de origen, si se incluyó en el evento.</summary>
        public string Database { get; set; } = string.Empty;
        /// <summary>Identificador de la conexión de origen, si se incluyó en el evento.</summary>
        public string ConnectionId { get; set; } = string.Empty;
        /// <summary>Tiempo de ejecución asociado al mensaje, si fue medido.</summary>
        public TimeSpan? ExecutionTime { get; set; }

        /// <summary>
        /// Devuelve una representación compacta del mensaje apta para consola o archivo de log.
        /// </summary>
        public override string ToString()
        {
            var level = LogLevel.ToString().ToUpper();
            var time = Timestamp.ToString("HH:mm:ss.fff");
            var proc = string.IsNullOrEmpty(Procedure) ? "N/A" : Procedure;

            return $"[{time}] [SQL-{level}] [Proc:{proc}:{LineNumber}] " +
                   $"[Err:{ErrorNumber}:{Severity}] {Message}";
        }

        /// <summary>
        /// Convierte la entrada de log a un diccionario plano, útil para serializarla o
        /// persistirla en un almacén estructurado (ej. tabla de logging SQL).
        /// Los valores opcionales no informados se incluyen como null.
        /// </summary>
        public Dictionary<string, object?> ToDictionary()
        {
            return new Dictionary<string, object?>
            {
                ["Timestamp"] = Timestamp,
                ["LogLevel"] = LogLevel.ToString(),
                ["Message"] = Message,
                ["Procedure"] = Procedure,
                ["LineNumber"] = LineNumber,
                ["ErrorNumber"] = ErrorNumber,
                ["Severity"] = Severity,
                ["State"] = State,
                ["Server"] = Server,
                ["Database"] = Database,
                ["ConnectionId"] = ConnectionId,
                ["ExecutionTimeMs"] = ExecutionTime?.TotalMilliseconds
            };
        }
    }
}
