using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Telemetry
{
    /// <summary>
    /// Define el contrato para el seguimiento de métricas y telemetría.
    /// </summary>
    /// <remarks>
    /// Esta interfaz permite registrar métricas de rendimiento, uso y salud del sistema
    /// para monitorear el comportamiento.
    /// </remarks>
    public interface IMetrics
    {
        /// <summary>
        /// Registra una métrica de tipo valor numérico.
        /// </summary>
        /// <param name="metricName">Nombre de la métrica a registrar.</param>
        /// <param name="value">Valor numérico de la métrica.</param>
        /// <param name="tags">Diccionario opcional de tags para categorizar la métrica.</param>
        void TrackMetric(string metricName, double value, IDictionary<string, string> tags = null);

        /// <summary>
        /// Registra el tiempo de ejecución de una operación.
        /// </summary>
        /// <param name="metricName">Nombre de la métrica de tiempo.</param>
        /// <param name="elapsedMilliseconds">Tiempo transcurrido en milisegundos.</param>
        /// <param name="tags">Diccionario opcional de tags para categorizar la métrica.</param>
        void TrackExecutionTime(string metricName, long elapsedMilliseconds, IDictionary<string, string>? tags = null);

        /// <summary>
        /// Incrementa un contador de métricas.
        /// </summary>
        /// <param name="metricName">Nombre del contador a incrementar.</param>
        /// <param name="value">Valor a incrementar (por defecto 1).</param>
        /// <param name="tags">Diccionario opcional de tags para categorizar la métrica.</param>
        void IncrementCounter(string metricName, int value = 1, IDictionary<string, string>? tags = null);

        /// <summary>
        /// Registra una excepción para seguimiento y análisis.
        /// </summary>
        /// <param name="exception">Excepción a registrar.</param>
        /// <param name="tags">Diccionario opcional de tags para categorizar el error.</param>
        void TrackException(Exception exception, IDictionary<string, string>? tags = null);

        /// <summary>
        /// Registra un evento personalizado.
        /// </summary>
        /// <param name="eventName">Nombre del evento.</param>
        /// <param name="properties">Propiedades adicionales del evento.</param>
        /// <param name="metrics">Métricas asociadas al evento.</param>
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    }
}
