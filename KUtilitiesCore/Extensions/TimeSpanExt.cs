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
        public static string ToReadableFormat(this TimeSpan ts)
        {
            if (ts.TotalMilliseconds < 1000)
            {
                return $"{ts.Milliseconds} ms";
            }
            else
            {
                // Define los formatos y sus umbrales basados en el total de segundos.
                // Los índices en las cadenas de formato corresponden a:
                // {0}: Días (ts.Days)
                // {1}: Horas (ts.Hours)
                // {2}: Minutos (ts.Minutes)
                // {3}: Segundos (ts.Seconds)
                // HmsFormatter se encarga de interpretar estos índices y los especificadores de formato (D, H, M, S).
                var cutoff = new SortedList<long, string>
                    {
                        {59, "{3:S}" }, // Menos de 1 minuto: muestra segundos
                        {60, "{2:M}" }, // Exactamente 1 minuto: muestra minutos
                        {60*60-1, "{2:M}, {3:S}"}, // Menos de 1 hora: muestra minutos y segundos
                        {60*60, "{1:H}"}, // Exactamente 1 hora: muestra horas
                        {24*60*60-1, "{1:H}, {2:M}"}, // Menos de 1 día: muestra horas y minutos
                        {24*60*60, "{0:D}"}, // Exactamente 1 día: muestra días
                        {Int64.MaxValue , "{0:D}, {1:H}"} // Más de 1 día: muestra días y horas
                    };
                // find nearest best match
                var find = cutoff.Keys.ToList()
                   .BinarySearch((long)ts.TotalSeconds);
                // negative values indicate a nearest match
                var near = find < 0 ? Math.Abs(find) - 1 : find;
                // use custom formatter to get the string
                return String.Format(
                    new HmsFormatter(),
                    cutoff[cutoff.Keys[near]],
                    ts.Days,
                    ts.Hours,
                    ts.Minutes,
                    ts.Seconds);
            }
        }
    }
}
