using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Telemetry
{
    /// <summary>
    /// Implementación de IMetrics que registra las métricas usando Delegados para Log.
    /// </summary>
    /// <remarks>
    /// Esta implementación es útil para entornos de desarrollo y testing, o cuando no se cuenta con
    /// un sistema de telemetría dedicado.
    /// </remarks>
    public class LoggerMetrics : IMetrics
    {
        #region Fields

        /// <summary>
        /// Delegado usado para registrar los mensajes en un LOG
        /// </summary>
        private readonly Action<MessageArgs> registerMessageDelegate;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="LoggerMetrics"/>.
        /// </summary>
        public LoggerMetrics(Action<MessageArgs>? registerMessageDelegate = null)
        {
            this.registerMessageDelegate = registerMessageDelegate ?? DefaultRegisterMessage;
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// Tipo de mensaje que se usa al regitrar
        /// </summary>
        public enum LoggerMetricsType
        {
            /// <summary>
            /// Tipo de mensaje de depuración
            /// </summary>
            Debug,

            /// <summary>
            /// Tipo de mensaje informativo
            /// </summary>
            Information,

            /// <summary>
            /// Tipo de mensaje de alarma
            /// </summary>
            Warning,

            /// <summary>
            /// Tipo de mensaje de error
            /// </summary>
            Error
        }

        #endregion Enums

        #region Methods

        /// <inheritdoc/>
        public void IncrementCounter(string metricName, int value = 1, IDictionary<string, string>? tags = null)
        {
            var logMessage = $"CONTADOR [{metricName}]: +{value}";

            if (tags != null && tags.Any())
            {
                logMessage += $" - Tags: {string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}"))}";
            }

            OnRegisterMessage(LoggerMetricsType.Debug, logMessage);
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            var logMessage = $"EVENTO [{eventName}]";

            if (properties != null && properties.Any())
            {
                logMessage += $" - Propiedades: {string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}"))}";
            }

            if (metrics != null && metrics.Any())
            {
                logMessage += $" - Métricas: {string.Join(", ", metrics.Select(m => $"{m.Key}={m.Value}"))}";
            }

            OnRegisterMessage(LoggerMetricsType.Information, logMessage);
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception, IDictionary<string, string>? tags = null)
        {
            var logMessage = "EXCEPCIÓN REGISTRADA";

            if (tags != null && tags.Any())
            {
                logMessage += $" - Tags: {string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}"))}";
            }

            OnRegisterMessage(LoggerMetricsType.Error, logMessage, exception);
        }

        /// <inheritdoc/>
        public void TrackExecutionTime(string metricName, long elapsedMilliseconds, IDictionary<string, string>? tags = null)
        {
            var logMessage = $"TIEMPO EJECUCIÓN [{metricName}]: {elapsedMilliseconds}ms";

            if (tags != null && tags.Any())
            {
                logMessage += $" - Tags: {string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}"))}";
            }

            // Usar diferentes niveles de log según el tiempo de ejecución
            if (elapsedMilliseconds > 1000)
            {
                OnRegisterMessage(LoggerMetricsType.Warning, logMessage);
            }
            else if (elapsedMilliseconds > 5000)
            {
                OnRegisterMessage(LoggerMetricsType.Error, logMessage);
            }
            else
            {
                OnRegisterMessage(LoggerMetricsType.Information, logMessage);
            }
        }

        /// <inheritdoc/>
        public void TrackMetric(string metricName, double value, IDictionary<string, string>? tags = null)
        {
            var logMessage = $"MÉTRICA [{metricName}]: {value}";

            if (tags != null && tags.Any())
            {
                logMessage += $" - Tags: {string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}"))}";
            }

            OnRegisterMessage(LoggerMetricsType.Information, logMessage);
        }

        private static void DefaultRegisterMessage(MessageArgs args)
        {
            switch (args.MessageType)
            {
                case LoggerMetricsType.Debug:
                    Debug.WriteLine(args.Message, "Debug");
                    break;

                case LoggerMetricsType.Information:
                    Debug.WriteLine(args.Message, "Info");
                    break;

                case LoggerMetricsType.Warning:
                    Debug.WriteLine(args.Message, "Warning");
                    break;

                case LoggerMetricsType.Error:
                    Debug.WriteLine($"{args.Message}\n{args.Exception?.Message}", "Error");
                    break;

                default:
                    break;
            }
        }

        private void OnRegisterMessage(LoggerMetricsType msgType, string message, Exception? ex = null)
        {
            registerMessageDelegate?.Invoke(new MessageArgs(msgType, message, ex));
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// Clase que provee información para registrar.
        /// </summary>
        public class MessageArgs
        {
            #region Constructors

            public MessageArgs(LoggerMetricsType messageType, string message, Exception? exception = null)
            {
                MessageType = messageType;
                Message = message;
                Exception = exception;
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Excepción del mensaje
            /// </summary>
            public Exception? Exception { get; }

            /// <summary>
            /// Texto del mensaje
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Indica el tipo de mensaje
            /// </summary>
            public LoggerMetricsType MessageType { get; }

            #endregion Properties
        }

        #endregion Classes
    }
}