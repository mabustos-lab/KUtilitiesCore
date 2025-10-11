using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Telemetry
{
    /// <summary>
    /// Implementación nula de IMetrics que no realiza ninguna acción.
    /// </summary>
    /// <remarks>
    /// Útil para testing o cuando no se desea rastrear métricas.
    /// </remarks>
    public class NullMetrics : IMetrics
    {
        /// <summary>
        /// Instancia singleton de NullMetrics.
        /// </summary>
        public static readonly NullMetrics Instance = new NullMetrics();

        private NullMetrics() { }

        /// <inheritdoc/>
        public void TrackMetric(string metricName, double value, IDictionary<string, string> tags = null)
        {
            // No hace nada
        }

        /// <inheritdoc/>
        public void TrackExecutionTime(string metricName, long elapsedMilliseconds, IDictionary<string, string> tags = null)
        {
            // No hace nada
        }

        /// <inheritdoc/>
        public void IncrementCounter(string metricName, int value = 1, IDictionary<string, string> tags = null)
        {
            // No hace nada
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception, IDictionary<string, string> tags = null)
        {
            // No hace nada
        }

        /// <inheritdoc/>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            // No hace nada
        }
    }
}
