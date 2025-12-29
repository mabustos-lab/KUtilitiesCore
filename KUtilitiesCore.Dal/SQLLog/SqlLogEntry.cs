using System;
using System.Linq;

namespace KUtilitiesCore.Dal.SQLLog
{
    public class SqlLogEntry
    {
        public DateTime Timestamp { get; set; }
        public SqlLogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string Procedure { get; set; }
        public int LineNumber { get; set; }
        public int ErrorNumber { get; set; }
        public int Severity { get; set; }
        public int State { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string ConnectionId { get; set; }
        public TimeSpan? ExecutionTime { get; set; }

        public override string ToString()
        {
            var level = LogLevel.ToString().ToUpper();
            var time = Timestamp.ToString("HH:mm:ss.fff");
            var proc = string.IsNullOrEmpty(Procedure) ? "N/A" : Procedure;

            return $"[{time}] [SQL-{level}] [Proc:{proc}:{LineNumber}] " +
                   $"[Err:{ErrorNumber}:{Severity}] {Message}";
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
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
