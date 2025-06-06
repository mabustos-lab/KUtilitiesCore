using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace KUtilitiesCore.Logger.Helpers
{
    internal struct LogEntry(DateTime timestamp, LogLevel level, EventId @event, Exception? exception, string message)
    {
        public EventId Event { get; set; } = @event;
        public Exception? Exception { get; set; } = exception;
        public LogLevel Level { get; set; } = level;
        public string Message { get; set; } = message;
        public DateTime Timestamp { get; set; } = timestamp;
        public override readonly string ToString()
        {
            return Message;
        }
    }
}