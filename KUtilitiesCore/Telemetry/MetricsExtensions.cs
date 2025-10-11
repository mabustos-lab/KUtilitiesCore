using System;
using System.Diagnostics;
using System.Linq;

namespace KUtilitiesCore.Telemetry
{
    /// <summary>
    /// Métodos de extensión para IMetrics para facilitar el uso común.
    /// </summary>
    public static class MetricsExtensions
    {
        /// <summary>
        /// Mide y registra el tiempo de ejecución de una acción.
        /// </summary>
        /// <param name="metrics">Instancia de IMetrics.</param>
        /// <param name="metricName">Nombre de la métrica.</param>
        /// <param name="action">Acción a medir.</param>
        /// <param name="tags">Tags adicionales.</param>
        public static void TimeAction(this IMetrics metrics, string metricName, Action action, IDictionary<string, string>? tags = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                stopwatch.Stop();
                metrics.TrackExecutionTime(metricName, stopwatch.ElapsedMilliseconds, tags);
            }
        }

        /// <summary>
        /// Mide y registra el tiempo de ejecución de una función asíncrona.
        /// </summary>
        /// <typeparam name="T">Tipo de retorno de la función.</typeparam>
        /// <param name="metrics">Instancia de IMetrics.</param>
        /// <param name="metricName">Nombre de la métrica.</param>
        /// <param name="func">Función asíncrona a medir.</param>
        /// <param name="tags">Tags adicionales.</param>
        /// <returns>El resultado de la función.</returns>
        public static async Task<T> TimeActionAsync<T>(this IMetrics metrics, string metricName, Func<Task<T>> func, IDictionary<string, string>? tags = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await func().ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();
                metrics.TrackExecutionTime(metricName, stopwatch.ElapsedMilliseconds, tags);
            }
        }

        /// <summary>
        /// Mide y registra el tiempo de ejecución de una acción asíncrona.
        /// </summary>
        /// <param name="metrics">Instancia de IMetrics.</param>
        /// <param name="metricName">Nombre de la métrica.</param>
        /// <param name="func">Acción asíncrona a medir.</param>
        /// <param name="tags">Tags adicionales.</param>
        public static async Task TimeActionAsync(this IMetrics metrics, string metricName, Func<Task> func, IDictionary<string, string>? tags = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                stopwatch.Stop();
                metrics.TrackExecutionTime(metricName, stopwatch.ElapsedMilliseconds, tags);
            }
        }
    }
}
