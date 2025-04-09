using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class DateTimeExt
    {
        // Constantes para aproximaciones de semanas, meses y años
        private const int DaysInWeek = 7;
        private const int DaysInMonth = 30;
        private const int DaysInYear = 365;

        /// <summary>
        /// Convierte un número de días en un texto que representa días, semanas, meses o años aproximados.
        /// </summary>
        /// <param name="days">Número de días a convertir.</param>
        /// <returns>Texto representando el tiempo aproximado.</returns>
        public static string ConvertDays(int days)
        {
            if (days < DaysInWeek)
            {
                return $"{days} días";
            }
            else if (days < DaysInMonth)
            {
                int weeks = days / DaysInWeek;
                return $"{weeks} semanas";
            }
            else if (days < DaysInYear)
            {
                int months = days / DaysInMonth;
                return $"{months} meses";
            }
            else
            {
                int years = days / DaysInYear;
                return $"{years} años";
            }
        }

        /// <summary>
        /// Obtiene la lista de nombres completos de los meses en el idioma del sistema actual.
        /// </summary>
        /// <returns>Lista de nombres de los meses.</returns>
        public static IEnumerable<string> GetMonthNames()
        {
            return CultureInfo.CurrentCulture.GetMonthNames();
        }

        /// <summary>
        /// Obtiene la lista de nombres abreviados de los meses en el idioma del sistema actual.
        /// </summary>
        /// <returns>Lista de nombres abreviados de los meses.</returns>
        public static IEnumerable<string> GetAbbreviatedMonthNames()
        {
            return CultureInfo.CurrentCulture.GetAbbreviatedMonthNames();
        }

        /// <summary>
        /// Obtiene la lista de nombres completos de los meses en el idioma especificado.
        /// </summary>
        /// <param name="ci">Cultura para obtener los nombres de los meses.</param>
        /// <returns>Lista de nombres de los meses.</returns>
        public static IEnumerable<string> GetMonthNames(this CultureInfo ci)
        {
            return Enumerable.Range(1, 12)
                .Select(m => ci.DateTimeFormat.GetMonthName(m));
        }

        /// <summary>
        /// Obtiene la lista de nombres abreviados de los meses en el idioma especificado.
        /// </summary>
        /// <param name="ci">Cultura para obtener los nombres abreviados de los meses.</param>
        /// <returns>Lista de nombres abreviados de los meses.</returns>
        public static IEnumerable<string> GetAbbreviatedMonthNames(this CultureInfo ci)
        {
            return Enumerable.Range(1, 12)
                .Select(m => ci.DateTimeFormat.GetAbbreviatedMonthName(m));
        }

        /// <summary>
        /// Obtiene el último día del mes de una fecha dada.
        /// </summary>
        /// <param name="dt">Fecha de entrada.</param>
        /// <returns>Último día del mes.</returns>
        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
        }

        /// <summary>
        /// Representa las partes de una fecha para cálculos de diferencia.
        /// </summary>
        public enum DatePart
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
        public enum QuarterPart
        {
            Q1 = 1,
            Q2 = 2,
            Q3 = 3,
            Q4 = 4
        }

        /// <summary>
        /// Calcula la diferencia entre dos fechas en la unidad especificada.
        /// </summary>
        /// <param name="StartDate">Fecha de inicio.</param>
        /// <param name="EndDate">Fecha de fin.</param>
        /// <param name="Part">Unidad de tiempo para la diferencia.</param>
        /// <param name="AbsoluteValue">Indica si el resultado debe ser absoluto.</param>
        /// <returns>Diferencia en la unidad especificada.</returns>
        public static long DateDiff(this DateTime StartDate, DateTime EndDate, DatePart Part, bool AbsoluteValue = false)
        {
            long DateDiffVal = 0;
            Calendar Cal = Thread.CurrentThread.CurrentCulture.Calendar;
            TimeSpan ts = new TimeSpan(checked(EndDate.Ticks - StartDate.Ticks));

            switch (Part)
            {
                case DatePart.Year:
                    DateDiffVal = Cal.GetYear(EndDate) - Cal.GetYear(StartDate);
                    break;
                case DatePart.Quarter:
                    DateDiffVal = ((Cal.GetYear(EndDate) - Cal.GetYear(StartDate)) * 4)
                        + ((Cal.GetMonth(EndDate) - 1) / 3)
                        - ((Cal.GetMonth(StartDate) - 1) / 3);
                    break;
                case DatePart.Month:
                    DateDiffVal = (Cal.GetYear(EndDate) - Cal.GetYear(StartDate)) * 12
                        + Cal.GetMonth(EndDate) - Cal.GetMonth(StartDate);
                    break;
                case DatePart.Day:
                    DateDiffVal = (long)ts.TotalDays + 1;
                    break;
                case DatePart.Week:
                    DateDiffVal = (long)ts.TotalDays / 7;
                    break;
                case DatePart.Hour:
                    DateDiffVal = (long)ts.TotalHours;
                    break;
                case DatePart.Minute:
                    DateDiffVal = (long)ts.TotalMinutes;
                    break;
                case DatePart.Second:
                    DateDiffVal = (long)ts.TotalSeconds;
                    break;
                case DatePart.Millisecond:
                    DateDiffVal = (long)ts.TotalMilliseconds;
                    break;
            }

            return AbsoluteValue ? Math.Abs(DateDiffVal) : DateDiffVal;
        }

        /// <summary>
        /// Calcula la diferencia entre dos fechas representadas como una tupla.
        /// </summary>
        /// <param name="RT">Tupla con la fecha de inicio y fin.</param>
        /// <param name="Part">Unidad de tiempo para la diferencia.</param>
        /// <returns>Diferencia en la unidad especificada.</returns>
        public static long DateDiff(this Tuple<DateTime, DateTime> RT, DatePart Part)
        {
            return RT.Item1.DateDiff(RT.Item2, Part, false);
        }

        /// <summary>
        /// Genera una colección de fechas por mes entre dos fechas.
        /// </summary>
        /// <param name="Ini">Fecha inicial.</param>
        /// <param name="fin">Fecha final.</param>
        /// <returns>Colección de fechas por mes.</returns>
        public static IEnumerable<DateTime> GetDateByMonth(this DateTime Ini, DateTime fin)
        {
            DateTime fromStart = new DateTime(Ini.Year, Ini.Month, 1);
            int TotalMonths = (int)(Ini.DateDiff(fin, DatePart.Month) + 1);
            return Enumerable.Range(0, TotalMonths).Select(x => fromStart.AddMonths(x));
        }

        /// <summary>
        /// Genera una colección de fechas por año entre dos fechas.
        /// </summary>
        /// <param name="Ini">Fecha inicial.</param>
        /// <param name="fin">Fecha final.</param>
        /// <returns>Colección de fechas por año.</returns>
        public static IEnumerable<DateTime> GetDateByYear(this DateTime Ini, DateTime fin)
        {
            DateTime fromStart = new DateTime(Ini.Year, Ini.Month, 1);
            int TotalYears = (int)(Ini.DateDiff(fin, DatePart.Year) + 1);
            return Enumerable.Range(0, TotalYears).Select(x => fromStart.AddYears(x));
        }

        /// <summary>
        /// Verifica si un año es bisiesto.
        /// </summary>
        /// <param name="dt">Fecha a verificar.</param>
        /// <returns>True si es bisiesto, de lo contrario False.</returns>
        public static bool IsLeapYear(this DateTime dt)
        {
            return DateTime.IsLeapYear(dt.Year);
        }

        /// <summary>
        /// Verifica si una fecha está dentro de un rango de fechas.
        /// </summary>
        /// <param name="dt">Fecha a verificar.</param>
        /// <param name="startDate">Fecha de inicio del rango.</param>
        /// <param name="endDate">Fecha de fin del rango.</param>
        /// <param name="compareTime">Indica si se debe comparar la hora.</param>
        /// <returns>True si está dentro del rango, de lo contrario False.</returns>
        public static bool IsBetween(this DateTime dt, DateTime startDate, DateTime endDate, bool compareTime = false)
        {
            return compareTime
                ? dt >= startDate && dt <= endDate
                : dt.Date >= startDate.Date && dt.Date <= endDate.Date;
        }
    }
    /// <summary>
    /// Representa el resultado de comparar un rango de fechas con otro rango.
    /// </summary>
    public class IsRangeDateBetweenResult(IsRangeDateBetweenResult.IsRangeDateBetween positionResult, int totalDaysIntoRange = 0)
    {
        /// <summary>
        /// Indica la posición relativa del rango comparado con otro rango.
        /// </summary>
        public IsRangeDateBetween PositionResult { get; } = positionResult;

        /// <summary>
        /// Representa el total de días que el rango comparado se encuentra dentro del otro rango.
        /// </summary>
        public int TotalDaysIntoRange { get; } = totalDaysIntoRange;

        /// <summary>
        /// Enumera las posibles posiciones relativas de un rango de fechas en comparación con otro.
        /// </summary>
        public enum IsRangeDateBetween
        {
            /// <summary>
            /// Indica que el rango está completamente antes del rango comparado.
            /// </summary>
            IsBeforeRange = -1,

            /// <summary>
            /// Indica que el rango está completamente después del rango comparado.
            /// </summary>
            IsAfterRange = -2,

            /// <summary>
            /// Indica que el rango está dentro del rango comparado.
            /// </summary>
            IsOnRange = 0
        }
    }
}
