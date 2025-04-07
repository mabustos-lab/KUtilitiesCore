using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Funcionalidades extendidas para algunas colecciones
    /// </summary>
    public static class CollectionExt
    {
        /// <summary>
        /// Ejecuta una acción por cada elemento de la colección.
        /// </summary>
        /// <typeparam name="T">El tipo de los elementos de la colección.</typeparam>
        /// <param name="collection">La colección a iterar.</param>
        /// <param name="iterationAction">La acción a ejecutar por cada elemento.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> iterationAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (iterationAction == null) throw new ArgumentNullException(nameof(iterationAction));

            foreach (T item in collection)
            {
                iterationAction(item);
            }
        }

        /// <summary>
        /// Ejecuta una acción por cada elemento de la colección, proporcionando el índice del elemento.
        /// </summary>
        /// <typeparam name="T">El tipo de los elementos de la colección.</typeparam>
        /// <param name="collection">La colección a iterar.</param>
        /// <param name="iterationAction">La acción a ejecutar por cada elemento, con el índice del elemento.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> iterationAction)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (iterationAction == null) throw new ArgumentNullException(nameof(iterationAction));

            int index = 0;
            foreach (T item in collection)
            {
                iterationAction(item, index++);
            }
        }
    }
}
