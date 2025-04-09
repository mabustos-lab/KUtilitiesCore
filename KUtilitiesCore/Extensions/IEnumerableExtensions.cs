using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Obtiene un orden aleatorio de los elementos de la colección.
        /// Implementa el algoritmo de Fisher-Yates (O(n) de complejidad)
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección</typeparam>
        /// <param name="source">Colección original que se va a mezclar</param>
        /// <param name="random">Generador de números aleatorios</param>
        /// <returns>La colección en orden aleatorio</returns>
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, Random random = null)
        {
            random = random ?? new Random();

            var list = source.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        /// <summary>
        /// Divide una colección en grupos de tamaño fijo (chunk).
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección a dividir.</param>
        /// <param name="chunkSize">Tamaño de cada grupo.</param>
        /// <returns>Enumeración de grupos, cada uno con el tamaño especificado.</returns>
        /// <exception cref="ArgumentException">Seroduce si el tamaño del grupo es menor o igual a cero.</exception>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("El tamaño debe ser mayor a cero.");
            }

            var list = source.ToList();
            int totalChunks = (int)Math.Ceiling((double)list.Count / chunkSize);

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * chunkSize;
                int endIndex = Math.Min(startIndex + chunkSize, list.Count);
                yield return list.GetRange(startIndex, endIndex - startIndex);
            }
        }
        /// <summary>
        /// Convierte una colección enumerada en un conjunto hash.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección original a convertir.</param>
        /// <param name="comparer">Implementación opcional de <see cref="IEqualityComparer{T}"/> para definir la comparación de elementos.</param>
        /// <returns>Un nuevo <see cref="HashSet{T}"/> que contiene los elementos de la colección original.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "La fuente no puede ser nula.");
            }

            return new HashSet<T>(source, comparer);
        }

    }
}
