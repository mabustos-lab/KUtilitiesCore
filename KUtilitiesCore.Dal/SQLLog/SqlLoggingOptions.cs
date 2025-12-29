using System;
using System.Linq;

namespace KUtilitiesCore.Dal.SQLLog
{
    public class SqlLoggingOptions
    {
        public SqlLogLevel MinimumLogLevel { get; set; } = SqlLogLevel.Info;
        public bool IncludeConnectionInfo { get; set; } = true;
        public bool IncludeTimestamp { get; set; } = true;
        public bool FilterSystemMessages { get; set; } = true;
        public int[] IgnoredErrorNumbers { get; set; } = Array.Empty<int>();
        public Func<SqlLogEntry, bool> CustomFilter { get; set; }
        public int MaxMessageLength { get; set; } = 4000;
    }
}
