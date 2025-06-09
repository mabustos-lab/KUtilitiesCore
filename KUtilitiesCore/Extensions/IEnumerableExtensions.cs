using KUtilitiesCore.Encryption;
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
        /// Obtiene un orden aleatorio de los elementos de la colección utilizando el algoritmo de Fisher-Yates (O(n)).
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección original que se va a mezclar.</param>
        /// <param name="random">Generador de números aleatorios (opcional). 
        /// Si no se proporciona en 
        /// .NET 8 o superior, utiliza <see cref="Random.Shared"/>
        /// y en .NET Framework 4.8, genera una semilla criptográficamente segura usando <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/>,
        /// y crea una nueva instancia de <see cref="Random"/> con dicha semilla para mejorar la aleatoriedad.</param>
        /// <returns>Nueva secuencia con los elementos en orden aleatorio.</returns>
        /// <exception cref="ArgumentNullException">Se produce si <paramref name="source"/> es nulo.</exception>
        public static T[] Randomize<T>(this IEnumerable<T> source, Random? random = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            random ??= GetPlatformSafeRandom();

            // Implementación optimizada para IList<T>
            if (source is IList<T> list)
                return RandomizeList(list, random);

            var buffer = source.ToArray();
            ShuffleInternal(buffer, random);
            return buffer;
        }

        /// <summary>
        /// Mezcla in-place una colección indexable utilizando el algoritmo de Fisher-Yates.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="list">Colección mutable a mezclar.</param>
        /// <param name="random">Generador de números aleatorios (opcional). 
        /// Si no se proporciona en 
        /// .NET 8 o superior, utiliza <see cref="Random.Shared"/>
        /// y en .NET Framework 4.8, genera una semilla criptográficamente segura usando <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/>,
        /// y crea una nueva instancia de <see cref="Random"/> con dicha semilla para mejorar la aleatoriedad.</param>
        /// <exception cref="ArgumentNullException">Se produce si <paramref name="list"/> es nulo.</exception>
        public static void ShuffleInPlace<T>(this IList<T> list, Random? random = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            random ??= GetPlatformSafeRandom();
            ShuffleInternal(list, random);
        }

        /// <summary>
        /// Método central reutilizable para el shuffle (algoritmo Fisher-Yates)
        /// </summary>
        private static void ShuffleInternal<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Implementación optimizada para IList<T>
        private static T[] RandomizeList<T>(IList<T> list, Random random)
        {
            // Creamos una copia para no modificar la original
            var copy = new T[list.Count];
            list.CopyTo(copy, 0);
            ShuffleInternal(copy, random);
            return copy;
        }

        /// <summary>
        /// Obtiene una instancia de <see cref="Random"/> adecuada para la plataforma y versión de .NET en uso.
        /// </summary>
        /// <remarks>
        /// En .NET 8 o superior, utiliza <c>Random.Shared</c>  para obtener una instancia compartida y eficiente.
        /// En .NET Framework 4.8, genera una semilla criptográficamente segura usando <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/>,
        /// y crea una nueva instancia de <see cref="Random"/> con dicha semilla para mejorar la aleatoriedad.
        /// </remarks>
        /// <returns>Una instancia de <see cref="Random"/> apropiada para la plataforma.</returns>
        private static Random GetPlatformSafeRandom()
        {
#if NET8_0_OR_GREATER
            return Random.Shared;
#elif NET48_OR_GREATER
            // Implementación criptográficamente segura para .NET 4.8
            return SaltGenerator.Random.Value;
#endif
        }

        /// <summary>
        /// Divide una colección en grupos de tamaño especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección a dividir.</param>
        /// <param name="chunkSize">Tamaño máximo de cada grupo.</param>
        /// <returns>Secuencia de grupos con el tamaño especificado.</returns>
        /// <exception cref="ArgumentException">Se produce si el tamaño del grupo es menor o igual a cero.</exception>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("El tamaño debe ser mayor a cero.", nameof(chunkSize));

#if NET8_0
        // Usar la implementación nativa de .NET 8+
        return Enumerable.Chunk(source, chunkSize);
#elif NET48
            // Implementación optimizada para .NET 4.8 con evaluación perezosa
            return ChunkLazy(source, chunkSize);
#endif
        }

#if NET48
        private static IEnumerable<IEnumerable<T>> ChunkLazy<T>(IEnumerable<T> source, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);
            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk;
                    chunk = new List<T>(chunkSize);
                }
            }

            if (chunk.Count > 0)
                yield return chunk;
        }
#endif
        /// <summary>
        /// Convierte una colección enumerada en un conjunto hash.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección original a convertir.</param>
        /// <param name="comparer">Implementación opcional de <see cref="IEqualityComparer{T}"/> para definir la comparación de elementos.</param>
        /// <returns>Un nuevo <see cref="HashSet{T}"/> que contiene los elementos de la colección original.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source), "La fuente no puede ser nula.");

            // Si comparer es nulo, se usa el constructor predeterminado de HashSet<T>
            return comparer is null ? [.. source] : new HashSet<T>(source, comparer);
        }

    }
}
