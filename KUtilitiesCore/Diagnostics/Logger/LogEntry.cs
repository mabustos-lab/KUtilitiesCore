using KUtilitiesCore.Diagnostics.Exceptions;

namespace KUtilitiesCore.Diagnostics.Logger
{
    internal class LogEntry
    {
        #region Properties

        public ExceptionInfo Exception { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        #endregion Properties
    }
}