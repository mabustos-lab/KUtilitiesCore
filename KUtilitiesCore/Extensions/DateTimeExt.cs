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
        public static string ConvertDaysToApproximateTime(int days)
        {
            return days switch
            {
                < DaysInWeek => $"{days} días",
                < DaysInMonth => $"{days / DaysInWeek} semanas",
                < DaysInYear => $"{days / DaysInMonth} meses",
                _ => $"{days / DaysInYear} años"
            };
        }

        /// <summary>
        /// Obtiene los nombres completos de los meses según la cultura actual.
        /// </summary>
        /// <returns>Secuencia de nombres de meses.</returns>
        public static IEnumerable<string> GetCurrentCultureMonthNames()
        {
            return CultureInfo.CurrentCulture.GetMonthNames();
        }

        /// <summary>
        /// Obtiene los nombres abreviados de los meses según la cultura actual.
        /// </summary>
        /// <returns>Secuencia de nombres abreviados de meses.</returns>
        public static IEnumerable<string> GetCurrentCultureAbbreviatedMonthNames()
        {
            return CultureInfo.CurrentCulture.GetAbbreviatedMonthNames();
        }

        /// <summary>
        /// Obtiene los nombres completos de los meses para una cultura específica.
        /// </summary>
        /// <param name="culture">Cultura a utilizar.</param>
        /// <returns>Secuencia de nombres de meses.</returns>
        public static IEnumerable<string> GetMonthNames(this CultureInfo culture)
        {
            ValidateCulture(culture);
            return GenerateMonthNames(culture, (c, m) => c.DateTimeFormat.GetMonthName(m));
        }

        /// <summary>
        /// Obtiene los nombres abreviados de los meses para una cultura específica.
        /// </summary>
        /// <param name="culture">Cultura a utilizar.</param>
        /// <returns>Secuencia de nombres abreviados de meses.</returns>
        public static IEnumerable<string> GetAbbreviatedMonthNames(this CultureInfo culture)
        {
            ValidateCulture(culture);
            return GenerateMonthNames(culture, (c, m) => c.DateTimeFormat.GetAbbreviatedMonthName(m));
        }

        /// <summary>
        /// Obtiene el último día del mes de la fecha especificada, conservando la hora.
        /// </summary>
        /// <param name="date">Fecha de referencia.</param>
        /// <returns>Fecha correspondiente al último día del mes.</returns>
        public static DateTime LastDayOfMonth(this DateTime date)
        {
            int lastDay = DateTime.DaysInMonth(date.Year, date.Month);
            return date.Day == lastDay && date.TimeOfDay == TimeSpan.Zero
                ? date
                : new DateTime(date.Year, date.Month, lastDay, date.Hour, date.Minute, date.Second, date.Millisecond, date.Kind);
        }

        /// <summary>
        /// Calcula la diferencia entre dos fechas en el intervalo especificado.
        /// </summary>
        /// <param name="startDate">Fecha inicial.</param>
        /// <param name="endDate">Fecha final.</param>
        /// <param name="interval">Tipo de intervalo para la diferencia.</param>
        /// <param name="absoluteValue">Si es true, retorna el valor absoluto.</param>
        /// <returns>Diferencia entre fechas según el intervalo.</returns>
        public static long CalculateDateDifference(this DateTime startDate, DateTime endDate, DateInterval interval, bool absoluteValue = false)
        {
            TimeSpan timeDifference = endDate - startDate;
            long difference = interval switch
            {
                DateInterval.Year => GetYearDifference(startDate, endDate),
                DateInterval.Quarter => GetQuarterDifference(startDate, endDate),
                DateInterval.Month => GetMonthDifference(startDate, endDate),
                DateInterval.Day => (long)timeDifference.TotalDays + 1,
                DateInterval.Week => (long)timeDifference.TotalDays / 7,
                DateInterval.Hour => (long)timeDifference.TotalHours,
                DateInterval.Minute => (long)timeDifference.TotalMinutes,
                DateInterval.Second => (long)timeDifference.TotalSeconds,
                DateInterval.Millisecond => (long)timeDifference.TotalMilliseconds,
                _ => throw new ArgumentOutOfRangeException(nameof(interval))
            };

            return absoluteValue ? Math.Abs(difference) : difference;
        }

        /// <summary>
        /// Genera una secuencia de fechas correspondientes al primer día de cada mes entre dos fechas.
        /// </summary>
        /// <param name="startDate">Fecha de inicio.</param>
        /// <param name="endDate">Fecha de fin.</param>
        /// <returns>Secuencia de fechas mensuales.</returns>
        public static IEnumerable<DateTime> GenerateMonthlyDates(this DateTime startDate, DateTime endDate)
        {
            DateTime normalizedStart = new(startDate.Year, startDate.Month, 1, 0, 0, 0, startDate.Kind);
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
        Year,
        Quarter,
        Month,
        Day,
        Week,
        Hour,
        Minute,
        Second,
        Millisecond
    }

    /// <summary>
    /// Representa los trimestres del año.
    /// </summary>
    public enum Quarter
    {
        Q1 = 1,
        Q2 = 2,
        Q3 = 3,
        Q4 = 4
    }

    /// <summary>
    /// Representa un rango de fechas.
    /// </summary>
    /// <param name="start">Fecha de inicio.</param>
    /// <param name="end">Fecha de fin.</param>
    public struct DateRange(DateTime start, DateTime end)
    {
        /// <summary>
        /// Fecha de inicio del rango.
        /// </summary>
        public DateTime Start { get; set; } = start;

        /// <summary>
        /// Fecha de fin del rango.
        /// </summary>
        public DateTime End { get; set; } = end;
    }

    /// <summary>
    /// Resultado de la comprobación de solapamiento entre dos rangos de fechas.
    /// </summary>
    /// <param name="position">Posición relativa del rango.</param>
    /// <param name="overlapDays">Días de solapamiento.</param>
    public class DateRangeOverlapResult(RangePosition position, int overlapDays = 0)
    {
        /// <summary>
        /// Posición relativa del rango respecto al rango de comparación.
        /// </summary>
        public RangePosition Position { get; } = position;

        /// <summary>
        /// Número de días de solapamiento.
        /// </summary>
        public int OverlapDays { get; } = overlapDays;
    }

    /// <summary>
    /// Posición relativa de un rango respecto a otro.
    /// </summary>
    public enum RangePosition
    {
        BeforeComparisonRange = -1,
        AfterComparisonRange = -2,
        Overlapping = 0
    }
}