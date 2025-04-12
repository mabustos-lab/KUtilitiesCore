using KUtilitiesCore.Diagnostics.Exceptions;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Collections.Concurrent;
using System.Text;

namespace KUtilitiesCore.Diagnostics.Logger
{
    public sealed class FileLoggerService : ILoggerService, IDisposable
    {
        #region Fields

        private readonly string _logFilePath;
        private readonly BlockingCollection<LogEntry> _logQueue = new BlockingCollection<LogEntry>();
        private readonly Task _processingTask;
        private readonly string _appName;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        private FileLoggerService(string logDirectory = null, string appName = "MyApplication")
        {
            _appName = appName;
            logDirectory = string.IsNullOrEmpty(logDirectory) ? GetDefaultLogDirectory(): logDirectory;
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

            _processingTask = Task.Factory.StartNew(
                ProcessLogQueue,
                TaskCreationOptions.LongRunning);
        }

        #endregion Constructors

        #region Methods

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
                    WriteToConsole(logMessage);
                    WriteToFile(logMessage);
                }
                catch (Exception ex)
                {
                    WriteToConsole($"Error processing log entry: {ex.Message}");
                }
            }
        }

        private void WriteToConsole(string message)
        {
            Console.WriteLine(message);
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
                WriteToConsole($"Error writing to log file: {ex.Message}");
            }
        }

        #endregion Methods
    }
}