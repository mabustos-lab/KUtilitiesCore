using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para trabajar con tipos de enumeraciones, 
    /// incluyendo operaciones con banderas y conversión de valores.
    /// </summary>
    public static class EnumsEx
    {
        /// <summary>
        /// Comprueba si una enumeración de banderas contiene un conjunto específico de banderas.
        /// </summary>
        /// <param name="source">La enumeración de origen.</param>
        /// <param name="value">El conjunto de banderas a comprobar.</param>
        /// <returns>True si la enumeración contiene las banderas especificadas; de lo contrario, false.</returns>
        public static bool HasFlag(this Enum source, Enum value)
        {
            if (source == null) return false;
            if (value == null) return false;
            return source.HasFlag(value);
        }
        /// <summary>
        /// Agrega un valor de bandera a la enumeración.
        /// </summary>
        /// <typeparam name="TEnum">El tipo de enumeración.</typeparam>
        /// <param name="source">La enumeración de origen.</param>
        /// <param name="value">El valor de bandera a agregar.</param>
        /// <returns>La enumeración resultante con la bandera agregada.</returns>
        /// <exception cref="ArgumentException">Si ocurre un error al agregar la bandera.</exception>
        public static TEnum Add<TEnum>(this TEnum source, TEnum value)
            where TEnum : Enum
        {
            try
            {
                return (TEnum)(object)((int)(object)source | (int)(object)value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("No se pudo agregar el valor del indicador {0} a la enumeración {1}", value, typeof(TEnum).Name), e);
            }
        }
        /// <summary>
        /// Elimina un valor de bandera de la enumeración.
        /// </summary>
        /// <typeparam name="TEnum">El tipo de enumeración.</typeparam>
        /// <param name="source">La enumeración de origen.</param>
        /// <param name="value">El valor de bandera a eliminar.</param>
        /// <returns>La enumeración resultante con la bandera eliminada.</returns>
        /// <exception cref="ArgumentException">Si ocurre un error al eliminar la bandera.</exception>
        public static TEnum Remove<TEnum>(this TEnum source, TEnum value)
             where TEnum : Enum
        {
            try
            {
                return (TEnum)(object)((int)(object)source & ~(int)(object)value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("No se pudo remover el valor del indicador {0} a la enumeración {1}", value, typeof(TEnum).Name), e);
            }
        }
        /// <summary>
        /// Establece el estado de una bandera en la enumeración.
        /// </summary>
        /// <typeparam name="TEnum">El tipo de enumeración.</typeparam>
        /// <param name="source">La enumeración de origen.</param>
        /// <param name="value">El valor de bandera a establecer.</param>
        /// <param name="activeFlag">True para activar la bandera; false para desactivarla.</param>
        /// <returns>La enumeración resultante con el estado de la bandera actualizado.</returns>
        public static TEnum SetFlag<TEnum>(this TEnum source, TEnum value, bool activeFlag)
             where TEnum : Enum
        {
            return activeFlag ? source.Add(value) : source.Remove(value);
        }
        /// <summary>
        /// Comprueba si el valor de la bandera es idéntico al valor de la enumeración proporcionada.
        /// </summary>
        /// <typeparam name="TEnum">El tipo de enumeración.</typeparam>
        /// <param name="source">La enumeración de origen.</param>
        /// <param name="value">El valor de bandera a comparar.</param>
        /// <returns>True si los valores son idénticos; de lo contrario, false.</returns>
        public static bool IsIdenticalFlag<TEnum>(this TEnum source, TEnum value)
            where TEnum : Enum
        {
            try
            {
                return (int)(object)source == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Convierte el tipo de enumeración proporcionado en una lista de valores.
        /// </summary>
        /// <typeparam name="TEnum">El tipo de enumeración.</typeparam>
        /// <returns>Una lista de valores de la enumeración.</returns>
        /// <exception cref="ArgumentException">Si el tipo proporcionado no es una enumeración.</exception>
        public static IEnumerable<TEnum> ToList<TEnum>()
            where TEnum : struct, Enum
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException();
            var values = Enum.GetNames(typeof(TEnum));
            return values.Select(value => value.ToEnum<TEnum>());
        }
    }
}
