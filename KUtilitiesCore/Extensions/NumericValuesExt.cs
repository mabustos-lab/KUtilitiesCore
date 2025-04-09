using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class NumericValuesExt
    {
        #region Methods

        /// <summary>
        /// Verifica si los elementos en una colección son consecutivos según una lógica definida.
        /// </summary>
        /// <typeparam name="T">El tipo de elementos en la colección. Debe ser comparable por igualdad.</typeparam>
        /// <param name="source">La colección de origen.</param>
        /// <param name="getNext">Una función que, dado un elemento, devuelve el siguiente elemento esperado en la secuencia consecutiva.</param>
        /// <returns>True si los elementos son consecutivos o si la colección está vacía o tiene un solo elemento; False en caso contrario.</returns>
        /// <exception cref="ArgumentNullException">Si source o getNext es null.</exception>
        public static bool AreConsecutive<T>(this IEnumerable<T> source, Func<T, T> getNext)
            where T : IEquatable<T> // Asegura que podamos comparar elementos T usando Equals()
        {
            // Validación de argumentos
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "La colección no puede ser nula.");
            }
            if (getNext == null)
            {
                throw new ArgumentNullException(nameof(getNext), "La función para obtener el siguiente elemento no puede ser nula.");
            }

            // Usar un enumerador para recorrer la colección eficientemente
            using (var enumerator = source.GetEnumerator())
            {
                // Si la colección está vacía, se considera consecutiva (trivialmente)
                if (!enumerator.MoveNext())
                {
                    return true;
                }

                // Guardar el primer elemento
                T previous = enumerator.Current;

                // Recorrer el resto de la colección
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;

                    // Calcular cuál debería ser el siguiente elemento esperado
                    T expectedNext = getNext(previous);

                    // Comparar el elemento actual con el esperado
                    // Usamos IEquatable<T>.Equals para la comparación
                    if (!current.Equals(expectedNext))
                    {
                        // Si no coincide, la secuencia no es consecutiva
                        return false;
                    }

                    // Actualizar el elemento anterior para la siguiente iteración
                    previous = current;
                }
            }

            // Si se recorrió toda la colección sin encontrar elementos no consecutivos, entonces es consecutiva
            return true;
        }

        /// <summary>
        /// Performs a rollup operation on the collection, applying the projection function sequentially.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
        /// <typeparam name="TResult">The type of the result after applying the projection.</typeparam>
        /// <param name="source">The collection to process.</param>
        /// <param name="seed">The initial value for the accumulator.</param>
        /// <param name="projection">A function that transforms the current element and the previous accumulator into the new accumulator.</param>
        /// <returns>A sequence of accumulated results.</returns>
        public static IEnumerable<TResult> Rollup<TSource, TResult>(
            this IEnumerable<TSource> source,
            TResult seed,
            Func<TSource, TResult, TResult> projection)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "The source collection cannot be null.");
            }

            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection), "The projection function cannot be null.");
            }

            List<TResult> results = new List<TResult>();
            TResult accumulator = seed;

            foreach (TSource item in source)
            {
                accumulator = projection(item, accumulator);
                results.Add(accumulator);
            }

            return results;
        }
        #endregion Methods
    }

}
