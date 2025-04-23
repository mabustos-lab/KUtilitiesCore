using KUtilitiesCore.Diagnostics.Exceptions;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Collections.Concurrent;
using System.Text;

namespace KUtilitiesCore.Diagnostics.Logger
{
    public sealed class FileLoggerService : ILoggerService, IDisposable
    {

        private readonly string _appName;
        private readonly string _logFilePath;
        private readonly BlockingCollection<LogEntry> _logQueue = new BlockingCollection<LogEntry>();
        private readonly Task _processingTask;
        private bool _disposed;

        private FileLoggerService(string logDirectory = null, string appName = "MyApplication")
        {
            _appName = appName;
            logDirectory = string.IsNullOrEmpty(logDirectory) ? GetDefaultLogDirectory() : logDirectory;
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"{appName}.log_{DateTime.Now:yyyyMMdd}.txt");
            ClearHistory(logDirectory);
            _processingTask = Task.Factory.StartNew(
                ProcessLogQueue,
                TaskCreationOptions.LongRunning);
        }

        public static ILoggerService Create(string logDirectory = null, string appName = "MyApplication")
            => new FileLoggerService(logDirectory, appName);

        public static ILoggerService Create(IO.SpecialStoreFolder folder = IO.SpecialStoreFolder.ApplicationData, string appName = "MyApplication")
            => new FileLoggerService(IO.StoreFolder.GetSpecialStoreFolder(folder), appName);

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _logQueue.CompleteAdding();
                _processingTask.Wait(TimeSpan.FromSeconds(5));
                _logQueue.Dispose();
            }
            finally
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void Log(LogLevel level, string message, Exception exception = null)
        {
            if (!_logQueue.IsAddingCompleted)
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message,
                    Exception = exception != null ? new ExceptionInfo(exception) : null
                };

                _logQueue.Add(logEntry);
            }
        }

        public void LogCritical(string message, Exception exception = null)
            => Log(LogLevel.Critical, message, exception);

        public void LogDebug(string message)
            => Log(LogLevel.Debug, message);

        public void LogError(string message, Exception exception = null)
            => Log(LogLevel.Error, message, exception);

        public void LogInformation(string message)
            => Log(LogLevel.Information, message);

        public void LogWarning(string message)
            => Log(LogLevel.Warning, message);

        private void ClearHistory(string logDirectory)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(logDirectory);
                var oldLogFiles = directoryInfo.GetFiles("*log*.txt")
                    .Where(file => file.CreationTime < DateTime.Now.AddDays(-30));

                foreach (var file in oldLogFiles)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"Error al eliminar archivos log historicos: {ex.Message}");
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();
            sb.Append($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Level}] {entry.Message}");

            if (entry.Exception != null)
            {
                sb.AppendLine();
                sb.Append(entry.Exception.GetReport());
            }

            return sb.ToString();
        }

        private string GetDefaultLogDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, _appName, "Logs");
        }

        private void ProcessLogQueue()
        {
            foreach (var entry in _logQueue.GetConsumingEnumerable())
            {
                try
                {
                    var logMessage = FormatLogEntry(entry);
                    WriteMessage(logMessage);
                    WriteToFile(logMessage);
                }
                catch (Exception ex)
                {
                    WriteMessage($"Error processing log entry: {ex.Message}");
                }
            }
        }

        private void WriteMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private void WriteToFile(string message)
        {
            try
            {
                using (var writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                WriteMessage($"Error writing to log file: {ex.Message}");
            }
        }

    }
}