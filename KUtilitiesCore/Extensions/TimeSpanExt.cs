using KUtilitiesCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class TimeSpanExt
    {
        /// <summary>
        /// Convierte un objeto <see cref="TimeSpan"/> en un formato legible por humanos.
        /// </summary>
        /// <param name="ts">El <see cref="TimeSpan"/> que se desea formatear.</param>
        /// <returns>
        /// Una cadena que representa el <see cref="TimeSpan"/> en un formato legible, como milisegundos, segundos, minutos, horas o días,
        /// dependiendo de la duración del intervalo de tiempo.
        /// </returns>
        public static string ToReadleFormat(this TimeSpan ts)
        {
            if (ts.TotalMilliseconds < 1000)
            {
                return $"{ts.Milliseconds} ms";
            }
            else
            {
                // formats and its cutoffs based on totalseconds
                var cutoff = new SortedList<long, string>
                    {
                        {59, "{3:S}" },
                        {60, "{2:M}" },
                        {60*60-1, "{2:M}, {3:S}"},
                        {60*60, "{1:H}"},
                        {24*60*60-1, "{1:H}, {2:M}"},
                        {24*60*60, "{0:D}"},
                        {Int64.MaxValue , "{0:D}, {1:H}"}
                    };
                // find nearest best match
                var find = cutoff.Keys.ToList()
                   .BinarySearch((long)ts.TotalSeconds);
                // negative values indicate a nearest match
                var near = find < 0 ? Math.Abs(find) - 1 : find;
                // use custom formatter to get the string
                return String.Format(
                    new HMSFormatter(),
                    cutoff[cutoff.Keys[near]],
                    ts.Days,
                    ts.Hours,
                    ts.Minutes,
                    ts.Seconds);
            }
        }
    }
}
