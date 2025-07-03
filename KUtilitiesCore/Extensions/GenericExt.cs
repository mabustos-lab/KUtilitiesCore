using System;
using System.Collections.Generic;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión genéricos para operaciones de rango y comparación sobre tipos numéricos y comparables.
    /// </summary>
    public static class GenericExt
    {
        /// <summary>
        /// Conjunto de tipos numéricos soportados para comprobaciones de rango.
        /// </summary>
        private static readonly HashSet<Type> NumericTypes =
            [
                typeof(decimal), typeof(double), typeof(float), typeof(int), typeof(long),
                typeof(short), typeof(sbyte), typeof(byte), typeof(ulong), typeof(uint),
                typeof(ushort), typeof(nint), typeof(nuint)
            ];

        /// <summary>
        /// Determina si un objeto comparable se encuentra dentro de un rango especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de valor que implementa <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="rangeStart">Extremo inicial del rango.</param>
        /// <param name="rangeEnd">Extremo final del rango.</param>
        /// <param name="inclusiveStart">Indica si el inicio del rango es inclusivo.</param>
        /// <param name="inclusiveEnd">Indica si el final del rango es inclusivo.</param>
        /// <returns>True si el valor está dentro del rango; de lo contrario, false.</returns>
        /// <exception cref="InvalidOperationException">Si el tipo no es soportado.</exception>
        public static bool IsInRange<T>(
            this T value,
            T rangeStart,
            T rangeEnd,
            bool inclusiveStart = true,
            bool inclusiveEnd = true) where T : struct, IComparable<T>
        {
            try
            {
                // Manejo de rangos invertidos
                (T lower, T upper) = OrderRange(rangeStart, rangeEnd);

                return IsNumericType<T>()
                    ? CheckNumericRange(value, lower, upper, inclusiveStart, inclusiveEnd)
                    : CheckComparableRange(value, lower, upper, inclusiveStart, inclusiveEnd);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Tipo no soportado: {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Determina si un valor <see cref="DateTime"/> está dentro de un rango especificado.
        /// </summary>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="rangeStart">Fecha de inicio del rango.</param>
        /// <param name="rangeEnd">Fecha de fin del rango.</param>
        /// <param name="inclusiveStart">Indica si el inicio del rango es inclusivo.</param>
        /// <param name="inclusiveEnd">Indica si el final del rango es inclusivo.</param>
        /// <returns>True si el valor está dentro del rango; de lo contrario, false.</returns>
        public static bool IsInRange(
            this DateTime value,
            DateTime rangeStart,
            DateTime rangeEnd,
            bool inclusiveStart = true,
            bool inclusiveEnd = true)
        {
            (DateTime lower, DateTime upper) = OrderRange(rangeStart, rangeEnd);

            bool lowerCondition = inclusiveStart ? value >= lower : value > lower;
            bool upperCondition = inclusiveEnd ? value <= upper : value < upper;

            return lowerCondition && upperCondition;
        }

        /// <summary>
        /// Ordena dos valores para determinar el menor y el mayor.
        /// </summary>
        /// <typeparam name="T">Tipo de valor comparable.</typeparam>
        /// <param name="a">Primer valor.</param>
        /// <param name="b">Segundo valor.</param>
        /// <returns>Tupla con el menor y el mayor valor.</returns>
        private static (T lower, T upper) OrderRange<T>(T a, T b) where T : struct, IComparable<T>
            => a.CompareTo(b) > 0 ? (b, a) : (a, b);

        /// <summary>
        /// Verifica si un valor numérico está dentro de un rango especificado.
        /// </summary>
        /// <typeparam name="T">Tipo numérico.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="lower">Límite inferior.</param>
        /// <param name="upper">Límite superior.</param>
        /// <param name="inclusiveStart">Indica si el límite inferior es inclusivo.</param>
        /// <param name="inclusiveEnd">Indica si el límite superior es inclusivo.</param>
        /// <returns>True si el valor está dentro del rango; de lo contrario, false.</returns>
        private static bool CheckNumericRange<T>(T value, T lower, T upper, bool inclusiveStart, bool inclusiveEnd)
            where T : struct
        {
            dynamic numValue = value;
            dynamic numLower = lower;
            dynamic numUpper = upper;

            bool lowerCondition = inclusiveStart ? numValue >= numLower : numValue > numLower;
            bool upperCondition = inclusiveEnd ? numValue <= numUpper : numValue < numUpper;

            return lowerCondition && upperCondition;
        }

        /// <summary>
        /// Verifica si un valor comparable está dentro de un rango especificado.
        /// </summary>
        /// <typeparam name="T">Tipo comparable.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="lower">Límite inferior.</param>
        /// <param name="upper">Límite superior.</param>
        /// <param name="inclusiveStart">Indica si el límite inferior es inclusivo.</param>
        /// <param name="inclusiveEnd">Indica si el límite superior es inclusivo.</param>
        /// <returns>True si el valor está dentro del rango; de lo contrario, false.</returns>
        private static bool CheckComparableRange<T>(T value, T lower, T upper, bool inclusiveStart, bool inclusiveEnd)
        {
            var comparer = Comparer<T>.Default;
            int lowerCompare = comparer.Compare(value, lower);
            int upperCompare = comparer.Compare(value, upper);

            return (inclusiveStart ? lowerCompare >= 0 : lowerCompare > 0) &&
                   (inclusiveEnd ? upperCompare <= 0 : upperCompare < 0);
        }

        /// <summary>
        /// Determina si un valor está entre dos valores (inclusivo).
        /// </summary>
        /// <typeparam name="T">Tipo comparable.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="lowerValue">Límite inferior.</param>
        /// <param name="upperValue">Límite superior.</param>
        /// <returns>True si el valor está entre los límites; de lo contrario, false.</returns>
        public static bool IsBetween<T>(this T value, T lowerValue, T upperValue) where T : struct, IComparable<T>
            => IsBetweenCore(value, lowerValue, upperValue, true, true);

        /// <summary>
        /// Determina si un valor está estrictamente entre dos valores (exclusivo).
        /// </summary>
        /// <typeparam name="T">Tipo comparable.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="lowerValue">Límite inferior.</param>
        /// <param name="upperValue">Límite superior.</param>
        /// <returns>True si el valor está estrictamente entre los límites; de lo contrario, false.</returns>
        public static bool IsBetweenExclusive<T>(this T value, T lowerValue, T upperValue) where T : struct, IComparable<T>
            => IsBetweenCore(value, lowerValue, upperValue, false, false);

        /// <summary>
        /// Lógica central para comprobaciones de rango entre dos valores.
        /// </summary>
        /// <typeparam name="T">Tipo comparable.</typeparam>
        /// <param name="value">Valor a evaluar.</param>
        /// <param name="lowerValue">Límite inferior.</param>
        /// <param name="upperValue">Límite superior.</param>
        /// <param name="inclusiveLower">Indica si el límite inferior es inclusivo.</param>
        /// <param name="inclusiveUpper">Indica si el límite superior es inclusivo.</param>
        /// <returns>True si el valor cumple la condición; de lo contrario, false.</returns>
        private static bool IsBetweenCore<T>(T value, T lowerValue, T upperValue, bool inclusiveLower, bool inclusiveUpper)
            where T : struct, IComparable<T>
        {
            (T lower, T upper) = OrderRange(lowerValue, upperValue);
            int lowerCompare = value.CompareTo(lower);
            int upperCompare = value.CompareTo(upper);

            return (inclusiveLower ? lowerCompare >= 0 : lowerCompare > 0) &&
                   (inclusiveUpper ? upperCompare <= 0 : upperCompare < 0);
        }

        /// <summary>
        /// Determina si el tipo especificado es numérico.
        /// </summary>
        /// <typeparam name="T">Tipo a comprobar.</typeparam>
        /// <returns>True si el tipo es numérico; de lo contrario, false.</returns>
        private static bool IsNumericType<T>()
        {
            Type type = typeof(T);
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return NumericTypes.Contains(underlyingType);
        }

        /// <summary>
        /// Determina si dos rangos se intersectan.
        /// </summary>
        /// <typeparam name="T">El tipo de los valores del rango, debe implementar <see cref="IComparable{T}"/>.</typeparam>
        /// <param name="x1">Inicio del primer rango.</param>
        /// <param name="y1">Fin del primer rango.</param>
        /// <param name="x2">Inicio del segundo rango.</param>
        /// <param name="y2">Fin del segundo rango.</param>
        /// <returns><c>true</c> si los rangos se intersectan; de lo contrario, <c>false</c>.</returns>
        /// <remarks>
        /// Se asume que los rangos están bien formados, es decir, x1 &lt;= y1 y x2 &lt;= y2.
        /// La intersección ocurre si el inicio del segundo rango es menor o igual al fin del primero,
        /// Y el inicio del primer rango es menor o igual al fin del segundo.
        /// </remarks>
        public static bool Intersect<T>(T x1, T y1, T x2, T y2) where T : IComparable<T>
            => x2.CompareTo(y1) <= 0 && x1.CompareTo(y2) <= 0;
    }
}