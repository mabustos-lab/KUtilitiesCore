using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace KUtilitiesCore.Logger
{
    public class DebugWindowLogger<TCategoryName> : LoggerServiceAbstract<TCategoryName>
    {
        internal override void WriteLog(
            LogLevel logLevel,
            EventId eventId,
            Exception? exception,
            string message,
            object[] args)
        {
            if(!IsEnabled(logLevel))
                return;

            try
            {
                var formattedMessage = string.Format(message, args);
                var eventIdInfo = eventId.Id != 0 ? $"| EventID: {eventId.Id} {eventId.Name}" : string.Empty;
                var logLine = $"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{categoryName}] {eventIdInfo} {formattedMessage}";

                if(exception != null)
                {
                    logLine += $"\n   {exception.GetType().Name}: {exception.Message}\n   {exception.StackTrace}";
                }

                Debug.WriteLine(logLine);
            } catch(Exception ex)
            {
                Debug.WriteLine($"[Error formatting log message] {ex.Message}");
            }
        }
        
    }
}
