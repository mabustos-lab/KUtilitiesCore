using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace KUtilitiesCore.Logger
{
    internal struct LogEntry
    {

        public LogEntry(DateTime timestamp, LogLevel level, EventId @event, Exception? exception, string message)
        {
            Timestamp = timestamp;
            Level = level;
            Event = @event;
            Exception = exception;
            Message = message;
        }

        public EventId Event { get; set; }
        public Exception? Exception { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public override string ToString()
        {
            return Message;
        }
    }
}