using System;
using System.Linq;

namespace KUtilitiesCore.Telemetry
{
    /// <summary>
    /// Implementación de IMetrics que combina múltiples proveedores de métricas.
    /// </summary>
    public class CompositeMetrics : IMetrics
    {
        private readonly IEnumerable<IMetrics> _metricsProviders;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CompositeMetrics"/>.
        /// </summary>
        /// <param name="metricsProviders">Colección de proveedores de métricas.</param>
        public CompositeMetrics(IEnumerable<IMetrics> metricsProviders)
        {
            _metricsProviders = metricsProviders?.ToList() ?? throw new ArgumentNullException(nameof(metricsProviders));
        }

        /// <inheritdoc/>
        public void TrackMetric(string metricName, double value, IDictionary<string, string>? tags = null)
        {
            foreach (var metrics in _metricsProviders)
            {
                metrics.TrackMetric(metricName, value, tags);
            }
        }

        /// <inheritdoc/>
        public void TrackExecutionTime(string metricName, long elapsedMilliseconds, IDictionary<string, string>? tags = null)
        {
            foreach (var metrics in _metricsProviders)
            {
                metrics.TrackExecutionTime(metricName, elapsedMilliseconds, tags);
            }
        }

        /// <inheritdoc/>
        public void IncrementCounter(string metricName, int value = 1, IDictionary<string, string>? tags = null)
        {
            foreach (var metrics in _metricsProviders)
            {
                metrics.IncrementCounter(metricName, value, tags);
            }
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception, IDictionary<string, string>? tags = null)
        {
            foreach (var metrics in _metricsProviders)
            {
                metrics.TrackException(exception, tags);
            }
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            foreach (var metricsProvider in _metricsProviders)
            {
                metricsProvider.TrackEvent(eventName, properties, metrics);
            }
        }
    }
}
