using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Logger que escribe en archivos con rotación automática y soporte para formato JSON. Permite
    /// registrar mensajes y excepciones en archivos de texto o JSON, con limpieza automática de
    /// logs antiguos y rotación por tamaño.
    /// </summary>
    /// <typeparam name="TCategoryName">Tipo para categorización de logs.</typeparam>
    public class FileLogger<TCategoryName> : LoggerServiceAbstract<TCategoryName>, IDisposable
    {

        private const int MaxRetries = 3;
        private const int RetryDelayMs = 100;
        private static readonly Mutex FileMutex = new(false, "Global\\FileLoggerMutex");

        private readonly string _applicationName;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _logDirectory;
        private readonly BlockingCollection<LogEntry> _logQueue = new();
        private readonly bool _useJsonFormat;
        private int _currentFileIndex = 1;
        private string _currentLogFilePath = string.Empty;

        private Task? _processingTask;
        private bool disposedValue;

        public FileLogger(
           string? logDirectory = null,
           string applicationName = "Application",
           bool useJsonFormat = false) : base(new FileLoggerOptions())
        {
            _applicationName = applicationName;
            _logDirectory = ValidateLogDirectory(logDirectory);
            _useJsonFormat = useJsonFormat;
            _jsonOptions = CreateJsonOptions();

            Directory.CreateDirectory(_logDirectory);
            InitializeLogFile();
            StartProcessingTask();
        }

        public FileLoggerOptions GetOptions
                    => (FileLoggerOptions)Options;

        internal override void WriteLog(LogEntry entry)
        {
            try
            {
                _logQueue.Add(entry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Queue Error] {ex.Message}");
            }
        }

        private static string FormatAsText(LogEntry entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level}] [{entry.Event.Id}] {entry.Message}");

            if (entry.Exception != null)
            {
                var exception = new ExceptionInfo(entry.Exception);
                sb.AppendLine($"EXCEPTION: {entry.Exception.GetType().Name}")
                  .AppendLine(exception.GetReport());
            }

            return sb.ToString();
        }

        private void ClearOldLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-GetOptions.RetentionDays);
                var pattern = _useJsonFormat ? "*.json" : "*.log";

                foreach (var file in Directory.GetFiles(_logDirectory, pattern))
                {
                    var info = new FileInfo(file);
                    if (info.CreationTime < cutoff)
                        info.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Cleanup Error] {ex.Message}");
            }
        }

        private JsonSerializerOptions CreateJsonOptions() => new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private string FormatAsJson(LogEntry entry)
        {
            var logObject = new
            {
                entry.Timestamp,
                Level = entry.Level.ToString(),
                Category = CategoryName,
                entry.Message,
                Exception = entry.Exception is null ? null : new ExceptionInfo(entry.Exception),
                EventId = entry.Event.Id,
                EventName = entry.Event.Name
            };

            return JsonSerializer.Serialize(logObject, _jsonOptions);
        }

        private string GenerateLogFilePath()
        {
            var dateStamp = DateTime.Now.ToString("yyyyMMdd");
            var extension = _useJsonFormat ? ".json" : ".log";
            return Path.Combine(_logDirectory, $"{_applicationName}_{dateStamp}_{_currentFileIndex:D2}{extension}");
        }

        private string GetDefaultLogDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, _applicationName, "Logs");
        }

        private void InitializeLogFile()
        {
            _currentLogFilePath = GenerateLogFilePath();
            ClearOldLogs();
        }

        private bool NeedsRotation()
        {
            var fileInfo = new FileInfo(_currentLogFilePath);
            return fileInfo.Exists && fileInfo.Length > GetOptions.MaxFileSizeBytes;
        }

        private void ProcessLogQueue()
        {
            foreach (var entry in _logQueue.GetConsumingEnumerable())
            {
                ProcessSingleEntry(entry);
            }
        }

        private void ProcessSingleEntry(LogEntry entry)
        {
            try
            {
                var content = _useJsonFormat
                    ? FormatAsJson(entry)
                    : FormatAsText(entry);

                SafeFileWrite(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Processing Error] {ex.Message}");
            }
        }

        private void RotateFile()
        {
            _currentFileIndex = (_currentFileIndex % GetOptions.MaxRetainedFiles) + 1;
            var newPath = GenerateLogFilePath();

            if (File.Exists(newPath))
                File.Delete(newPath);

            File.Move(_currentLogFilePath, newPath);
        }

        private void SafeFileWrite(string content)
        {
            var success = false;
            var attempt = 0;

            while (!success && attempt++ < MaxRetries)
            {
                try
                {
                    FileMutex.WaitOne();

                    if (NeedsRotation())
                        RotateFile();

                    File.AppendAllText(_currentLogFilePath, content + Environment.NewLine, Encoding.UTF8);
                    success = true;
                }
                catch (IOException ex) when (attempt < MaxRetries)
                {
                    Thread.Sleep(RetryDelayMs);
                    Debug.WriteLine($"[IO Retry {attempt}] {ex.Message}");
                }
                finally
                {
                    FileMutex.ReleaseMutex();
                }
            }

            if (!success)
                Debug.WriteLine("[Critical] Failed to write log entry");
        }

        private void StartProcessingTask()
        {
            _processingTask = Task.Factory.StartNew(
                ProcessLogQueue,
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private string ValidateLogDirectory(string? logDirectory)
        {
            try
            {
                // 1. Usar directorio por defecto si no se proporciona
                var finalPath = string.IsNullOrWhiteSpace(logDirectory)
                    ? GetDefaultLogDirectory()
                    : logDirectory!.Trim();

                // 2. Normalizar la ruta (eliminar caracteres extraños)
                finalPath = Path.GetFullPath(finalPath);

                // 3. Crear directorio si no existe
                if (!Directory.Exists(finalPath))
                {
                    Directory.CreateDirectory(finalPath);
                    Debug.WriteLine($"[Logger] Directorio creado: {finalPath}");
                }

                return finalPath;
            }
            catch (Exception ex)
            {
                // 4. Fallback a directorio temporal en caso de error crítico
                var tempPath = Path.Combine(Path.GetTempPath(), _applicationName, "Logs");
                Directory.CreateDirectory(tempPath);
                Debug.WriteLine($"[Logger Error] Usando directorio temporal: {tempPath}. Razón: {ex.Message}");
                return tempPath;
            }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logQueue.CompleteAdding();
                    _processingTask?.Wait(TimeSpan.FromSeconds(5));
                    _logQueue.Dispose();
                    //FileMutex.Dispose();
                }


                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}