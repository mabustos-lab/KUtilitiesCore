using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace KUtilitiesCore.Logger
{
    public class DebugWindowLogger<TCategoryName> : LoggerServiceAbstract<TCategoryName>
    {
        internal override void WriteLog(LogEntry entry)
        {
            Debug.WriteLine(entry.ToString());
            if (entry.Exception != null)
            {
                var exception = new ExceptionInfo(entry.Exception);
                Debug.WriteLine(exception.GetReport());
            }
        }
    }
}
