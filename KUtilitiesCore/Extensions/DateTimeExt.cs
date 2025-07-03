using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para operaciones avanzadas con <see cref="DateTime"/> y utilidades relacionadas con fechas.
    /// </summary>
    public static class DateTimeExtensions
    {
        private const int DaysInWeek = 7;
        private const int DaysInMonth = 30;
        private const int DaysInYear = 365;

        /// <summary>
        /// Convierte una cantidad de días en una cadena que representa el tiempo aproximado en días, semanas, meses o años.
        /// </summary>
        /// <param name="days">Cantidad de días.</param>
        /// <returns>Cadena representando el tiempo aproximado.</returns>
        /// <remarks>
        /// Para la conversión a meses, se considera un mes de 30 días.
        /// Para la conversión a años, se considera un año de 365 días.
        /// Estas son aproximaciones para simplificar la representación.
        /// </remarks>
        public static string ConvertDaysToApproximateTime(int days)
        {
            // Se utilizan constantes para definir la duración aproximada de semana, mes y año.
            // Esto hace que el código sea más legible y facilita el ajuste de estas aproximaciones si es necesario.
            return days switch
            {
                // Si los días son menos que una semana, se muestran en días.
                < DaysInWeek => $"{days} días",
                // Si los días son menos que un mes (aprox.), se muestran en semanas.
                < DaysInMonth => $"{days / DaysInWeek} semanas",
                // Si los días son menos que un año (aprox.), se muestran en meses.
                < DaysInYear => $"{days / DaysInMonth} meses",
                // Para cantidades mayores, se muestran en años.
                _ => $"{days / DaysInYear} años"
            };
        }

        /// <summary>
        /// Obtiene los nombres completos de los meses según la cultura actual del hilo (<see cref="CultureInfo.CurrentCulture"/>).
        /// </summary>
        /// <returns>Una secuencia <see cref="IEnumerable{String}"/> con los nombres de los meses, desde el primero hasta el duodécimo.
        /// El último elemento de la secuencia puede ser una cadena vacía si la cultura tiene 13 meses.</returns>
        public static IEnumerable<string> GetCurrentCultureMonthNames()
        {
            return CultureInfo.CurrentCulture.GetMonthNames();
        }

        /// <summary>
        /// Obtiene los nombres abreviados de los meses según la cultura actual del hilo (<see cref="CultureInfo.CurrentCulture"/>).
        /// </summary>
        /// <returns>Una secuencia <see cref="IEnumerable{String}"/> con los nombres abreviados de los meses.
        /// El último elemento de la secuencia puede ser una cadena vacía si la cultura tiene 13 meses.</returns>
        public static IEnumerable<string> GetCurrentCultureAbbreviatedMonthNames()
        {
            return CultureInfo.CurrentCulture.GetAbbreviatedMonthNames();
        }

        /// <summary>
        /// Obtiene los nombres completos de los meses para una cultura específica.
        /// </summary>
        /// <param name="culture">La cultura (<see cref="CultureInfo"/>) para la cual se obtendrán los nombres de los meses.</param>
        /// <returns>Una secuencia <see cref="IEnumerable{String}"/> con los nombres de los meses.
        /// El último elemento de la secuencia puede ser una cadena vacía si la cultura tiene 13 meses.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="culture"/> es <c>null</c>.</exception>
        public static IEnumerable<string> GetMonthNames(this CultureInfo culture)
        {
            ValidateCulture(culture);
            return GenerateMonthNames(culture, (c, m) => c.DateTimeFormat.GetMonthName(m));
        }

        /// <summary>
        /// Obtiene los nombres abreviados de los meses para una cultura específica.
        /// </summary>
        /// <param name="culture">La cultura (<see cref="CultureInfo"/>) para la cual se obtendrán los nombres abreviados de los meses.</param>
        /// <returns>Una secuencia <see cref="IEnumerable{String}"/> con los nombres abreviados de los meses.
        /// El último elemento de la secuencia puede ser una cadena vacía si la cultura tiene 13 meses.</returns>
        /// <exception cref="ArgumentNullException">Si <paramref name="culture"/> es <c>null</c>.</exception>
        public static IEnumerable<string> GetAbbreviatedMonthNames(this CultureInfo culture)
        {
            ValidateCulture(culture);
            return GenerateMonthNames(culture, (c, m) => c.DateTimeFormat.GetAbbreviatedMonthName(m));
        }

        /// <summary>
        /// Obtiene el último día del mes de la fecha especificada, conservando la hora, minutos, segundos, milisegundos y <see cref="DateTimeKind"/>.
        /// </summary>
        /// <param name="date">Fecha de referencia.</param>
        /// <returns>Un <see cref="DateTime"/> que representa el último día del mes de <paramref name="date"/>, con la misma hora y <see cref="DateTimeKind"/>.</returns>
        public static DateTime LastDayOfMonth(this DateTime date)
        {
            int lastDay = DateTime.DaysInMonth(date.Year, date.Month);
            // Optimización: Si la fecha ya es el último día del mes y la hora es medianoche (00:00:00),
            // se devuelve la misma instancia para evitar crear un nuevo objeto DateTime innecesariamente.
            // Esto es útil si la fecha ya representa exactamente el fin del día.
            // Sin embargo, si se requiere que la hora sea siempre la original de 'date', esta condición de TimeOfDay debería quitarse
            // o ajustarse para que siempre cree un nuevo DateTime con la hora original si el día cambia.
            // La implementación actual conserva la hora original de 'date' al construir el nuevo DateTime.
            return date.Day == lastDay //Si ya es el ultimo dia del mes y no tiene componente de hora.
                ? date
                : new DateTime(date.Year, date.Month, lastDay, date.Hour, date.Minute, date.Second, date.Millisecond, date.Kind);
        }

        /// <summary>
        /// Calcula la diferencia entre dos fechas en el intervalo especificado.
        /// </summary>
        /// <param name="startDate">Fecha inicial del período.</param>
        /// <param name="endDate">Fecha final del período.</param>
        /// <param name="interval">El tipo de intervalo (<see cref="DateInterval"/>) en el que se calculará la diferencia (ej. años, meses, días).</param>
        /// <param name="absoluteValue">Si es <c>true</c>, el resultado será el valor absoluto de la diferencia, asegurando que no sea negativo. El valor predeterminado es <c>false</c>.</param>
        /// <returns>La diferencia entre <paramref name="endDate"/> y <paramref name="startDate"/> en las unidades especificadas por <paramref name="interval"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Si <paramref name="interval"/> no es un valor válido de <see cref="DateInterval"/>.</exception>
        public static long CalculateDateDifference(this DateTime startDate, DateTime endDate, DateInterval interval, bool absoluteValue = false)
        {
            TimeSpan timeDifference = endDate - startDate;
            long difference = interval switch
            {
                DateInterval.Year => GetYearDifference(startDate, endDate),
                DateInterval.Quarter => GetQuarterDifference(startDate, endDate),
                DateInterval.Month => GetMonthDifference(startDate, endDate),
                // Para días, se suma 1 para que la diferencia sea inclusiva.
                // Por ejemplo, la diferencia entre hoy y hoy es 1 día.
                // Si se quisiera una diferencia exclusiva (número de días completos entre las fechas), no se sumaría 1.
                DateInterval.Day => (long)timeDifference.TotalDays + 1,
                DateInterval.Week => (long)timeDifference.TotalDays / 7, // Representa semanas completas.
                DateInterval.Hour => (long)timeDifference.TotalHours,
                DateInterval.Minute => (long)timeDifference.TotalMinutes,
                DateInterval.Second => (long)timeDifference.TotalSeconds,
                DateInterval.Millisecond => (long)timeDifference.TotalMilliseconds,
                _ => throw new ArgumentOutOfRangeException(nameof(interval))
            };

            return absoluteValue ? Math.Abs(difference) : difference;
        }

        /// <summary>
        /// Genera una secuencia de fechas, donde cada fecha es el primer día de cada mes comprendido entre <paramref name="startDate"/> y <paramref name="endDate"/>, inclusive.
        /// </summary>
        /// <param name="startDate">La fecha de inicio del período. La parte de día y hora de esta fecha se normaliza al primer día del mes a las 00:00:00.</param>
        /// <param name="endDate">La fecha de fin del período.</param>
        /// <returns>Una secuencia <see cref="IEnumerable{DateTime}"/> de fechas, cada una representando el primer día de un mes dentro del rango especificado.</returns>
        /// <remarks>
        /// La secuencia incluirá el primer día del mes de <paramref name="startDate"/> y, si <paramref name="endDate"/> cae en un mes posterior,
        /// el primer día de cada mes intermedio, hasta el primer día del mes de <paramref name="endDate"/> inclusive.
        /// </remarks>
        public static IEnumerable<DateTime> GenerateMonthlyDates(this DateTime startDate, DateTime endDate)
        {
            // Normaliza la fecha de inicio al primer día del mes, manteniendo la información de Kind.
            DateTime normalizedStart = new(startDate.Year, startDate.Month, 1, 0, 0, 0, startDate.Kind);
            // Calcula el número total de meses en el rango, incluyendo el mes de inicio y el de fin.
            // Se suma 1 porque CalculateDateDifference(..., DateInterval.Month) da la diferencia de meses completos.
            // Por ejemplo, de Enero a Enero es 0, pero queremos generar 1 fecha (Enero).
            int totalMonths = (int)startDate.CalculateDateDifference(endDate, DateInterval.Month) + 1;
            return Enumerable.Range(0, totalMonths).Select(n => normalizedStart.AddMonths(n));
        }

        /// <summary>
        /// Verifica si dos rangos de fechas se solapan y retorna información sobre la superposición.
        /// </summary>
        /// <param name="targetRange">Rango objetivo.</param>
        /// <param name="comparisonRange">Rango a comparar.</param>
        /// <returns>Resultado de la superposición de rangos.</returns>
        public static DateRangeOverlapResult CheckDateRangeOverlap(this DateRange targetRange, DateRange comparisonRange)
        {
            if (targetRange.End < comparisonRange.Start)
                return new DateRangeOverlapResult(RangePosition.AfterComparisonRange);

            if (comparisonRange.End < targetRange.Start)
                return new DateRangeOverlapResult(RangePosition.BeforeComparisonRange);

            DateTime overlapStart = new[] { targetRange.Start, comparisonRange.Start }.Max();
            DateTime overlapEnd = new[] { targetRange.End, comparisonRange.End }.Min();
            int overlapDays = (int)overlapStart.CalculateDateDifference(overlapEnd, DateInterval.Day);

            return new DateRangeOverlapResult(RangePosition.Overlapping, overlapDays);
        }

        #region Helper Methods
        /// <summary>
        /// Genera los nombres de los meses usando un selector personalizado.
        /// </summary>
        /// <param name="culture">Cultura a utilizar.</param>
        /// <param name="nameSelector">Función para seleccionar el nombre del mes.</param>
        /// <returns>Secuencia de nombres de meses.</returns>
        private static IEnumerable<string> GenerateMonthNames(CultureInfo culture, Func<CultureInfo, int, string> nameSelector)
        {
            return Enumerable.Range(1, 12)
                .Select(month => nameSelector(culture, month));
        }

        /// <summary>
        /// Valida que la cultura no sea nula.
        /// </summary>
        /// <param name="culture">Cultura a validar.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void ValidateCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));
        }

        /// <summary>
        /// Calcula la diferencia en años entre dos fechas.
        /// </summary>
        private static long GetYearDifference(DateTime start, DateTime end)
        {
            return end.Year - start.Year;
        }

        /// <summary>
        /// Calcula la diferencia en trimestres entre dos fechas.
        /// </summary>
        private static long GetQuarterDifference(DateTime start, DateTime end)
        {
            return ((end.Year - start.Year) * 4) + ((end.Month - 1) / 3 - (start.Month - 1) / 3);
        }

        /// <summary>
        /// Calcula la diferencia en meses entre dos fechas.
        /// </summary>
        private static long GetMonthDifference(DateTime start, DateTime end)
        {
            return (end.Year - start.Year) * 12 + (end.Month - start.Month);
        }
        #endregion
    }

    /// <summary>
    /// Intervalos de tiempo para cálculos de diferencia de fechas.
    /// </summary>
    public enum DateInterval
    {
        /// <summary>
        /// Diferencia en años completos.
        /// </summary>
        Year,
        /// <summary>
        /// Diferencia en trimestres completos.
        /// </summary>
        Quarter,
        /// <summary>
        /// Diferencia en meses completos.
        /// </summary>
        Month,
        /// <summary>
        /// Diferencia en días. Considera días calendario inclusivos.
        /// </summary>
        Day,
        /// <summary>
        /// Diferencia en semanas completas.
        /// </summary>
        Week,
        /// <summary>
        /// Diferencia en horas completas.
        /// </summary>
        Hour,
        /// <summary>
        /// Diferencia en minutos completos.
        /// </summary>
        Minute,
        /// <summary>
        /// Diferencia en segundos completos.
        /// </summary>
        Second,
        /// <summary>
        /// Diferencia en milisegundos completos.
        /// </summary>
        Millisecond
    }

    /// <summary>
    /// Representa los trimestres del año.
    /// </summary>
    public enum Quarter
    {
        /// <summary>
        /// Primer trimestre (Enero, Febrero, Marzo).
        /// </summary>
        Q1 = 1,
        /// <summary>
        /// Segundo trimestre (Abril, Mayo, Junio).
        /// </summary>
        Q2 = 2,
        /// <summary>
        /// Tercer trimestre (Julio, Agosto, Septiembre).
        /// </summary>
        Q3 = 3,
        /// <summary>
        /// Cuarto trimestre (Octubre, Noviembre, Diciembre).
        /// </summary>
        Q4 = 4
    }

    /// <summary>
    /// Representa un rango de fechas definido por una fecha de inicio y una fecha de fin.
    /// </summary>
    /// <param name="start">La fecha de inicio del rango.</param>
    /// <param name="end">La fecha de fin del rango.</param>
    public struct DateRange(DateTime start, DateTime end)
    {
        /// <summary>
        /// Obtiene o establece la fecha de inicio del rango.
        /// </summary>
        public DateTime Start { get; set; } = start;

        /// <summary>
        /// Obtiene o establece la fecha de fin del rango.
        /// </summary>
        public DateTime End { get; set; } = end;
    }

    /// <summary>
    /// Encapsula el resultado de una comprobación de solapamiento entre dos rangos de fechas.
    /// </summary>
    /// <param name="position">La posición relativa del rango objetivo con respecto al rango de comparación (ej. si solapa, está antes, o después).</param>
    /// <param name="overlapDays">El número de días que los rangos se solapan. Es 0 si no hay solapamiento.</param>
    public class DateRangeOverlapResult(RangePosition position, int overlapDays = 0)
    {
        /// <summary>
        /// Obtiene la posición relativa del rango objetivo con respecto al rango de comparación.
        /// </summary>
        public RangePosition Position { get; } = position;

        /// <summary>
        /// Obtiene el número de días de solapamiento entre los dos rangos.
        /// Si los rangos no se solapan, este valor es 0.
        /// </summary>
        public int OverlapDays { get; } = overlapDays;
    }

    /// <summary>
    /// Define la posición relativa de un rango de fechas (<see cref="DateRange"/>) con respecto a otro rango con el que se compara.
    /// </summary>
    public enum RangePosition
    {
        /// <summary>
        /// Indica que el rango objetivo termina antes de que comience el rango de comparación (sin solapamiento).
        /// </summary>
        BeforeComparisonRange = -1,
        /// <summary>
        /// Indica que el rango objetivo comienza después de que termina el rango de comparación (sin solapamiento).
        /// </summary>
        AfterComparisonRange = -2,
        /// <summary>
        /// Indica que el rango objetivo se solapa parcial o totalmente con el rango de comparación.
        /// </summary>
        Overlapping = 0
    }
}