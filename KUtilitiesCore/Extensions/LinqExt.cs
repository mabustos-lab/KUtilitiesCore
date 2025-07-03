using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class LinqExt
    {
        #region Methods

        /// <summary>
        /// Transpone una colección de IEnumerables.
        /// La primera fila se convierte en la primera columna, la segunda fila en la segunda columna, etc.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la colección.</typeparam>
        /// <param name="source">Colección a transponer.</param>
        /// <returns>Nueva colección transpuesta.</returns>
        /// <exception cref="ArgumentNullException">Se produce si la fuente o cualquier fila son nulas.</exception>
        /// <remarks>
        /// Si las secuencias de entrada tienen diferentes longitudes, la transposición se detendrá cuando la secuencia más corta se agote.
        /// Las columnas resultantes tendrán una longitud igual a la de la secuencia de entrada más corta.
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "La fuente no puede ser nula.");
            }

            var enumerators = source.Select(row =>
            {
                if (row == null)
                {
                    throw new ArgumentNullException(nameof(row), "Ninguna fila puede ser nula.");
                }

                return row.GetEnumerator();
            }).ToArray();

            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }

        #endregion Methods
    }
}
