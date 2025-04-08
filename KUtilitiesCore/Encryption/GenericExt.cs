using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Encryption
{
  
    public static class GenericExt
    {
        /// <summary>
        /// Determina si un objeto comparable se encuentra dentro de un rango especificado.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto que implementa IComparable.</typeparam>
        /// <param name="value">Valor a comparar.</param>
        /// <param name="rangeStart">Límite inferior del rango.</param>
        /// <param name="rangeEnd">Límite superior del rango.</param>
        /// <param name="inclusiveStart">Indica si el límite inferior es incluyente.</param>
        /// <param name="inclusiveEnd">Indica si el límite superior es incluyente.</param>
        /// <returns>true si el valor está dentro del rango; de lo contrario, false.</returns>
        public static bool IsInRange<T>(
            this T value,
            T rangeStart,
            T rangeEnd,
            bool inclusiveStart = true,
            bool inclusiveEnd = true)
            where T : struct
        {
            try
            {
                var comparer = Comparer<T>.Default;
                return ValidateNumericRange(value, rangeStart, rangeEnd, inclusiveStart, inclusiveEnd, comparer)
                       || ValidateComparableRange(value, rangeStart, rangeEnd, inclusiveStart, inclusiveEnd, comparer);
            }
            catch (NotSupportedException)
            {
                return false;                
            }
        }

        /// <summary>
        /// Valida si un valor numérico se encuentra dentro de un rango.
        /// </summary>
        /// <typeparam name="T">Tipo numérico del valor.</typeparam>
        /// <param name="value">Valor a comparar.</param>
        /// <param name="rangeStart">Límite inferior del rango.</param>
        /// <param name="rangeEnd">Límite superior del rango.</param>
        /// <param name="inclusiveStart">Indica si el límite inferior es incluyente.</param>
        /// <param name="inclusiveEnd">Indica si el límite superior es incluyente.</param>
        /// <param name="comparer">Objeto para comparar los valores.</param>
        /// <returns>true si el valor está dentro del rango numérico; de lo contrario, false.</returns>
        private static bool ValidateNumericRange<T>(
            T value,
            T rangeStart,
            T rangeEnd,
            bool inclusiveStart,
            bool inclusiveEnd,
            IComparer<T> comparer)
        {
            if (!IsNumericType<T>())
                return false;

            var numValue = Convert.ToDecimal(value);
            var numStart = Convert.ToDecimal(rangeStart);
            var numEnd = Convert.ToDecimal(rangeEnd);

            return (inclusiveStart ? numValue >= numStart : numValue > numStart) &&
                   (inclusiveEnd ? numValue <= numEnd : numValue < numEnd);
        }

        /// <summary>
        /// Valida si un valor comparable se encuentra dentro de un rango.
        /// </summary>
        /// <typeparam name="T">Tipo comparable del valor.</typeparam>
        /// <param name="value">Valor a comparar.</param>
        /// <param name="rangeStart">Límite inferior del rango</param>
        /// <param name="rangeEnd">Límite superior del rango</param>
        /// <param name="inclusiveStart">Indica si el límite inferior es incluyente</param>
        /// <param name="inclusiveEnd">Indica si el límite superior es incluyente</param>
        /// <param name="comparer">Objeto para comparar los valores</param>
        /// <returns>true si el valor está dentro del rango; de lo contrario, false.</returns>
        private static bool ValidateComparableRange<T>(
            T value,
            T rangeStart,
            T rangeEnd,
            bool inclusiveStart,
            bool inclusiveEnd,
            IComparer<T> comparer)
        {
            var startComparison = comparer.Compare(value, rangeStart);
            var endComparison = comparer.Compare(value, rangeEnd);

            var startCondition = inclusiveStart ? startComparison >= 0 : startComparison > 0;
            var endCondition = inclusiveEnd ? endComparison <= 0 : endComparison < 0;

            return startCondition && endCondition;
        }

        /// <summary>
        /// Verifica si un tipo dado es un tipo numérico.
        /// </summary>
        /// <typeparam name="T">Tipo a verificar.</typeparam>
        /// <returns>true si el tipo es numérico; de lo contrario, false.</returns>
        private static bool IsNumericType<T>() =>
            typeof(T) == typeof(decimal) ||
            typeof(T) == typeof(decimal?) ||
            typeof(T) == typeof(double) ||
            typeof(T) == typeof(double?) ||
            typeof(T) == typeof(float) ||
            typeof(T) == typeof(float?) ||
            typeof(T) == typeof(int) ||
            typeof(T) == typeof(int?)
            || typeof(T) == typeof(long?) || typeof(T) == typeof(long) // Mejora: Se podría extender para otros tipos numéricos
    ;
    }
}