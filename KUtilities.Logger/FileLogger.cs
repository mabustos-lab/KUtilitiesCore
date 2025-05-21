using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Logger que escribe en archivos con rotación automática y soporte para formato JSON.
    /// Permite registrar mensajes y excepciones en archivos de texto o JSON, con limpieza automática de logs antiguos y rotación por tamaño.
    /// </summary>
    /// <typeparam name="TCategoryName">Tipo para categorización de logs.</typeparam>
    public class FileLogger<TCategoryName> : LoggerServiceAbstract<TCategoryName>, IDisposable
    {
        /// <summary>
        /// Nombre de la aplicación para identificar los archivos de log.
        /// </summary>
        private readonly string _applicationName;

        /// <summary>
        /// Ruta actual del archivo de log en uso.
        /// </summary>
        private readonly string _currentLogFilePath;

        /// <summary>
        /// Opciones de serialización JSON para los logs.
        /// </summary>
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Directorio donde se almacenan los archivos de log.
        /// </summary>
        private readonly string _logDirectory;

        /// <summary>
        /// Cola concurrente para almacenar las entradas de log pendientes de escritura.
        /// </summary>
        private readonly BlockingCollection<LogEntry> _logQueue = [];

        /// <summary>
        /// Tarea en segundo plano que procesa la cola de logs.
        /// </summary>
        private readonly Task _processingTask;

        /// <summary>
        /// Indica si se utiliza formato JSON para los logs.
        /// </summary>
        private readonly bool _useJsonFormat;

        /// <summary>
        /// Índice del archivo de log actual para la rotación.
        /// </summary>
        private int _currentFileIndex = 1;

        /// <summary>
        /// Indica si el logger ya fue liberado.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Constructor del FileLogger.
        /// </summary>
        /// <param name="logDirectory">Directorio de logs (opcional).</param>
        /// <param name="applicationName">Nombre de la aplicación.</param>
        /// <param name="useJsonFormat">Si es true, usa formato JSON.</param>
        public FileLogger(string? logDirectory = null,
                        string applicationName = "Application",
                        bool useJsonFormat = false) : base(new FileLoggerOptions())
        {
            _applicationName = applicationName;
            _logDirectory = GetValidLogDirectory(logDirectory);
            _useJsonFormat = useJsonFormat;

            Directory.CreateDirectory(_logDirectory);
            _currentLogFilePath = GenerateLogFilePath();
            ClearOldLogs();

            _processingTask = Task.Factory.StartNew(
                ProcessLogQueue,
                TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Obtiene las opciones específicas de FileLogger.
        /// </summary>
        private FileLoggerOptions GetOptions => (FileLoggerOptions)Options;

        /// <summary>
        /// Libera los recursos utilizados por el logger.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Escribe una entrada de log en la cola para su posterior procesamiento.
        /// </summary>
        /// <param name="logLevel">Nivel de log.</param>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        /// <param name="message">Mensaje de log.</param>
        /// <param name="args">Argumentos para el mensaje.</param>
        internal override void WriteLog(
            LogLevel logLevel,
            EventId eventId,
            Exception? exception,
            string message,
            object[] args)
        {
            if (!IsEnabled(logLevel)) return;

            try
            {
                var formattedMessage = string.Format(message, args);
                var timestamp = DateTime.Now;

                _logQueue.Add(new FileLogger<TCategoryName>.LogEntry(
                    timestamp,
                    logLevel,
                    eventId,
                    exception,
                    formattedMessage));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Formatting Error] {ex.Message}");
            }
        }

        /// <summary>
        /// Libera los recursos utilizados por el logger.
        /// </summary>
        /// <param name="disposing">Indica si se está liberando explícitamente.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _logQueue.CompleteAdding();
                _processingTask.Wait(TimeSpan.FromSeconds(5));
                _logQueue.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Elimina archivos de log antiguos según la política de retención.
        /// </summary>
        private void ClearOldLogs()
        {
            try
            {
                var retentionDate = DateTime.Now.AddDays(-30);
                var logFiles = new DirectoryInfo(_logDirectory)
                    .GetFiles(_useJsonFormat ? "*.json" : "*.log")
                    .Where(f => f.CreationTime < retentionDate);

                foreach (var file in logFiles)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Cleanup Error] {ex.Message}");
            }
        }

        /// <summary>
        /// Formatea una entrada de log como JSON.
        /// </summary>
        /// <param name="entry">Entrada de log.</param>
        /// <returns>Cadena JSON representando la entrada.</returns>
        private string FormatJsonLogEntry(LogEntry entry)
        {
            var logRecord = new
            {
                entry.Timestamp,
                Level = entry.Level.ToString(),
                Category = CategoryName,
                Message = entry.Message ?? string.Empty,
                Exception = entry.Exception != null ? new ExceptionInfo(entry.Exception) : null,
                EventId = entry.Event.Id,
                EventName = entry.Event.Name
            };

            return JsonSerializer.Serialize(logRecord, _jsonOptions);
        }

        /// <summary>
        /// Formatea una entrada de log como texto plano.
        /// </summary>
        /// <param name="entry">Entrada de log.</param>
        /// <returns>Cadena de texto representando la entrada.</returns>
        private string FormatTextLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level}] [{CategoryName}] {entry.Message}");

            if (entry.Exception != null)
            {
                ExceptionInfo exceptionInfo = new(entry.Exception);
                sb.AppendLine()
                    .AppendLine(exceptionInfo.GetReport());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Genera la ruta del archivo de log actual según la configuración y el índice.
        /// </summary>
        /// <returns>Ruta del archivo de log.</returns>
        private string GenerateLogFilePath()
        {
            var baseName = $"{_applicationName}_{DateTime.Now:yyyyMMdd}";
            var extension = _useJsonFormat ? ".json" : ".log";
            return Path.Combine(_logDirectory, $"{baseName}_{_currentFileIndex:D2}{extension}");
        }

        /// <summary>
        /// Obtiene el directorio de logs por defecto.
        /// </summary>
        /// <returns>Ruta del directorio de logs.</returns>
        private string GetDefaultLogDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, _applicationName, "Logs");
        }

        /// <summary>
        /// Valida y obtiene el directorio de logs a utilizar.
        /// </summary>
        /// <param name="logDirectory">Directorio proporcionado.</param>
        /// <returns>Directorio válido.</returns>
        private string GetValidLogDirectory(string? logDirectory)
        {
            return string.IsNullOrEmpty(logDirectory)
                ? GetDefaultLogDirectory()
                : logDirectory!;
        }

        /// <summary>
        /// Procesa la cola de logs y escribe cada entrada en el archivo correspondiente.
        /// </summary>
        private void ProcessLogQueue()
        {
            foreach (var entry in _logQueue.GetConsumingEnumerable())
            {
                try
                {
                    var formattedMessage = _useJsonFormat
                        ? FormatJsonLogEntry(entry)
                        : FormatTextLogEntry(entry);

                    WriteToFile(formattedMessage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Log Processing Error] {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Realiza la rotación del archivo de log si se supera el tamaño máximo configurado.
        /// </summary>
        private void RotateLogFileIfNeeded()
        {
            try
            {
                var fileInfo = new FileInfo(_currentLogFilePath);

                if (!fileInfo.Exists || fileInfo.Length < GetOptions.MaxFileSizeBytes)
                    return;

                _currentFileIndex = _currentFileIndex % GetOptions.MaxRetainedFiles + 1;
                var newFilePath = GenerateLogFilePath();

                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);

                File.Move(_currentLogFilePath, newFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Log Rotation Error] {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe el mensaje formateado en el archivo de log.
        /// </summary>
        /// <param name="message">Mensaje a escribir.</param>
        private void WriteToFile(string message)
        {
            try
            {
                RotateLogFileIfNeeded();

                using var writer = new StreamWriter(_currentLogFilePath, true, Encoding.UTF8);
                writer.WriteLine(message);

                if (_useJsonFormat && !message.EndsWith(","))
                    writer.WriteLine(","); // Separador para múltiples entradas JSON
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[File Write Error] {ex.Message}");
            }
        }

        /// <summary>
        /// Estructura interna que representa una entrada de log.
        /// </summary>
        /// <remarks>
        /// Inicializa una nueva instancia de LogEntry.
        /// </remarks>
        /// <param name="timestamp">Fecha y hora del log.</param>
        /// <param name="level">Nivel de log.</param>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="exception">Excepción asociada.</param>
        /// <param name="message">Mensaje de log.</param>
        private readonly struct LogEntry(
            DateTime timestamp,
            LogLevel level,
            EventId eventId,
            Exception? exception,
            string message)
        {

            /// <summary>
            /// Id del evento de log.
            /// </summary>
            public EventId Event { get; } = eventId;

            /// <summary>
            /// Excepción asociada al log (si existe).
            /// </summary>
            public Exception? Exception { get; } = exception;

            /// <summary>
            /// Nivel de severidad del log.
            /// </summary>
            public LogLevel Level { get; } = level;

            /// <summary>
            /// Mensaje del log.
            /// </summary>
            public string Message { get; } = message;

            /// <summary>
            /// Fecha y hora del log.
            /// </summary>
            public DateTime Timestamp { get; } = timestamp;
        }
    }
}