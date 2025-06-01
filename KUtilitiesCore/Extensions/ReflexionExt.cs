using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    /// <summary>
    /// Proporciona extensiones de reflexión para trabajar con tipos y propiedades.
    /// </summary>
    public static class ReflectionExtensions
    {

        /// <summary>
        /// Obtiene una colección de <see cref="PropertyInfo"/> para las propiedades del tipo especificado.
        /// </summary>
        /// <param name="type">El tipo del cual se obtenerán las propiedades.</param>
        /// <param name="onlySupportedTypes">Indica si solo se incluirán tipos soportados.</param>
        /// <param name="filter">Un predicado para filtrar las propiedades.</param>
        /// <returns>Una colección de <see cref="PropertyInfo"/>.</returns>
        public static IEnumerable<PropertyInfo> GetPropertiesInfo(this Type type,
            bool onlySupportedTypes = true,
            Func<PropertyInfo, bool>? filter = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var properties = type.GetProperties();

            return ApplyPropertyFilters(properties, onlySupportedTypes, filter);
        }

        /// <summary>
        /// Obtiene una colección de <see cref="PropertyInfo"/> para las propiedades de la instancia
        /// del objeto especificado.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto.</typeparam>
        /// <param name="obj">La instancia del objeto.</param>
        /// <param name="onlySupportedTypes">Indica si solo se incluirán tipos soportados.</param>
        /// <param name="filter">Un predicado para filtrar las propiedades.</param>
        /// <returns>Una colección de <see cref="PropertyInfo"/>.</returns>
        public static IEnumerable<PropertyInfo> GetPropertiesInfo<T>(this T obj,
            bool onlySupportedTypes = true,
            Func<PropertyInfo, bool>? filter = null)
        {
            return GetPropertiesInfo(typeof(T), onlySupportedTypes, filter);
        }

        /// <summary>
        /// Obtiene una colección de <see cref="PropertyInfo"/> filtrada por nombre para las
        /// propiedades del tipo especificado.
        /// </summary>
        /// <param name="type">El tipo del cual se obtenerán las propiedades.</param>
        /// <param name="onlySupportedTypes">Indica si solo se incluirán tipos soportados.</param>
        /// <param name="nameFilter">Un predicado para filtrar las propiedades por su nombre.</param>
        /// <returns>Una colección de <see cref="PropertyInfo"/>.</returns>
        public static IEnumerable<PropertyInfo> GetPropertiesInfoFilteredByName(this Type type,
            bool onlySupportedTypes = true,
            Func<string, bool>? nameFilter = null)
        {
            return GetPropertiesInfoCore(type, onlySupportedTypes, nameFilter);
        }

        /// <summary>
        /// Obtiene una colección de <see cref="PropertyInfo"/> con los filtros aplicados.
        /// </summary>
        /// <param name="type">El tipo del cual se obtenerán las propiedades.</param>
        /// <param name="onlySupportedTypes">Indica si solo se incluirán tipos soportados.</param>
        /// <param name="nameFilter">Un predicado para filtrar las propiedades por su nombre.</param>
        /// <returns>Una colección de <see cref="PropertyInfo"/>.</returns>
        internal static IEnumerable<PropertyInfo> GetPropertiesInfoCore(Type type,
            bool onlySupportedTypes,
            Func<string, bool>? nameFilter = null)
        {
            var properties = type.GetProperties();

            if (nameFilter != null)
            {
                properties = properties.Where(p => nameFilter.Invoke(p.Name))
                    .ToArray();
            }

            return onlySupportedTypes
                ? ApplyTypeFilters(properties)
                : properties;
        }

        /// <summary>
        /// Aplica filtros de tipos soportados y filtro personalizado a una colección de <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="properties">La colección de propiedades a filtrar.</param>
        /// <param name="onlySupportedTypes">Indica si solo se incluirán tipos soportados.</param>
        /// <param name="filter">Un predicado para filtrar las propiedades.</param>
        /// <returns>Una colección filtrada de <see cref="PropertyInfo"/>.</returns>
        private static IEnumerable<PropertyInfo> ApplyPropertyFilters(IEnumerable<PropertyInfo> properties,
            bool onlySupportedTypes,
            Func<PropertyInfo, bool>? filter = null)
        {
            var filtered = properties;

            if (onlySupportedTypes)
            {
                filtered = ApplyTypeFilters(filtered);
            }

            if (filter != null)
            {
                filtered = filtered.Where(filter);
            }

            return filtered;
        }

        /// <summary>
        /// Aplica filtros de tipos soportados a una colección de <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="properties">La colección de propiedades a filtrar.</param>
        /// <returns>Una colección filtrada de <see cref="PropertyInfo"/>.</returns>
        private static IEnumerable<PropertyInfo> ApplyTypeFilters(IEnumerable<PropertyInfo> properties)
        {
            var supportedTypes = GetSupportedTypes();

            return properties.Where(p =>
                IsSupportedType(p.PropertyType, supportedTypes));
        }

        /// <summary>
        /// Obtiene los tipos primitivos y de fecha soportados.
        /// </summary>
        /// <returns>Una colección de tipos soportados.</returns>
        private static IEnumerable<Type> GetSupportedTypes()
        {
            yield return typeof(bool);
            yield return typeof(char);
            yield return typeof(sbyte);
            yield return typeof(byte);
            yield return typeof(short);
            yield return typeof(ushort);
            yield return typeof(int);
            yield return typeof(uint);
            yield return typeof(long);
            yield return typeof(ulong);
            yield return typeof(float);
            yield return typeof(double);
            yield return typeof(decimal);
            yield return typeof(DateTime);
            yield return typeof(string);
            yield return typeof(Enum);
        }

        /// <summary>
        /// Verifica si un tipo es compatible con los tipos soportados.
        /// </summary>
        /// <param name="type">El tipo a verificar.</param>
        /// <param name="supportedTypes">Los tipos soportados.</param>
        /// <returns>True si el tipo es compatible, false en caso contrario.</returns>
        private static bool IsSupportedType(Type type, IEnumerable<Type> supportedTypes)
        {
            var underlyingType = type.GetUnderlyingType();

            return underlyingType != null && supportedTypes.Contains(underlyingType,
                new TypeComparer()) ||
                   supportedTypes.Contains(type,
                        new TypeComparer());
        }

        /// <summary>
        /// Implementa <see graphicre="IEqualityComparer{Type}"/> para comparar tipos.
        /// </summary>
        private sealed class TypeComparer : IEqualityComparer<Type>
        {

            /// <summary>
            /// Compara dos tipos para determinar si son iguales.
            /// </summary>
            /// <param name="x">El primer tipo a comparar.</param>
            /// <param name="y">El segundo tipo a comparar.</param>
            /// <returns>True si los tipos son iguales, false en caso contrario.</returns>
            public bool Equals(Type? x, Type? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.Equals(y);
            }

            /// <summary>
            /// Obtiene el código hash del tipo.
            /// </summary>
            /// <param name="obj">El tipo a obtener el código hash.</param>
            /// <returns>El código hash del tipo.</returns>
            public int GetHashCode(Type obj)
            {
                return obj.GetHashCode();
            }

        }

    }
}