using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    /// <summary>
    /// Valida un objeto que implementa IDataErrorInfo
    /// </summary>
    public static class DataErrorInfoExt
    {

        /// <summary>
        /// Obtiene el texto de error para una propiedad específica de un objeto.
        /// </summary>
        /// <param name="owner">El objeto del cual se intentará obtener la propiedad.</param>
        /// <param name="propertyName">
        /// El nombre de la propiedad, el cual puede contener puntos para navegar por propiedades anidadas.
        /// </param>
        /// <returns>
        /// El texto de error correspondiente a la propiedad, o una cadena vacía si no se encuentra.
        /// </returns>
        public static string GetErrorText(this object owner, string propertyName)
        {
            if (owner is null) 
                throw new ArgumentNullException(nameof(owner));

            if (propertyName.Contains('.'))
            {
                return GetNestedPropertyErrorText(owner, propertyName);
            }

            return GetNonNestedErrorText(owner, propertyName);
        }

        /// <summary>
        /// Verifica si un objeto que implementa IDataErrorInfo tiene errores.
        /// </summary>
        /// <param name="owner">El objeto que implementa IDataErrorInfo.</param>
        /// <param name="ignoreOwnerError">Indica si se debe omitir el error del propietario propio.</param>
        /// <param name="deep">El nivel de profundidad para buscar errores en propiedades anidadas.</param>
        /// <returns>true si se encuentran errores; en caso contrario, false.</returns>
        public static bool HasErrors<T>(T owner, bool ignoreOwnerError, int deep = 2)
            where T : class, IDataErrorInfo
        {
            if (owner is null)
                throw new ArgumentNullException(nameof(owner));
            
            if (deep < 1) return false;

            var properties = GetRelevantProperties(owner, ignoreOwnerError);
            return properties.Any(p => PropertyHasError(owner, p, deep) || !ignoreOwnerError && !string.IsNullOrEmpty(owner.Error));
        }

        /// <summary>
        /// Verifica si un objeto que implementa IDataErrorInfo tiene errores, con un valor
        /// predeterminado para ignoreOwnerError.
        /// </summary>
        /// <param name="owner">El objeto que implementa IDataErrorInfo.</param>
        /// <param name="deep">El nivel de profundidad para buscar errores en propiedades anidadas.</param>
        /// <returns>true si se encuentran errores; en caso contrario, false.</returns>
        public static bool HasErrors(IDataErrorInfo owner, int deep = 2)
            => HasErrors(owner, false, deep);

        /// <summary>
        /// Obtiene todos los atributos de un miembro.
        /// </summary>
        /// <param name="member">El miembro del cual se recuperarán los atributos.</param>
        /// <returns>Una matriz de atributos asociados al miembro.</returns>
        private static IEnumerable<Attribute> GetAllAttributes(MemberInfo member)
            => Attribute.GetCustomAttributes(member, false).AsEnumerable();

        /// <summary>
        /// Obtiene el texto de error para una propiedad anidada.
        /// </summary>
        /// <param name="owner">El objeto raíz.</param>
        /// <param name="propertyPath">El camino de la propiedad, separado por puntos.</param>
        /// <returns>El texto de error de la propiedad anidada, o una cadena vacía si no se encuentra.</returns>
        private static string GetNestedPropertyErrorText(object owner, string propertyPath)
        {
            var split = propertyPath.Split('.');
            if (split.Length < 2) return string.Empty;

            if (!TryGetPropertyValue(owner, split[0], out var nestedObject)) return string.Empty;
            if (nestedObject is not IDataErrorInfo dataErrorInfo) return string.Empty;

            var nestedPath = string.Join(".", split.Skip(1));
            return dataErrorInfo[nestedPath];
        }

        /// <summary>
        /// Obtiene el texto de error para una propiedad no anidada.
        /// </summary>
        /// <param name="obj">El objeto.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>El texto de error de la propiedad, o null si no se encuentra.</returns>
        private static string GetNonNestedErrorText(object obj, string propertyName)
        {
            var type = obj.GetType();
            var propertyValidator = GetPropertyValidator(type, propertyName);
            if (propertyValidator is null) return string.Empty;

            if (!TryGetPropertyValue(obj, propertyName, out var propertyValue)) 
                return string.Empty;
            else
                return propertyValidator.GetValidationErrorMessage(propertyValue, obj);
        }

        /// <summary>
        /// Obtiene el validador de propiedades basado en los atributos del miembro.
        /// </summary>
        /// <param name="type">El tipo del objeto.</param>
        /// <param name="propertyName">El nombre de la propiedad.</param>
        /// <returns>El validador de propiedades, o null si no se encuentra.</returns>
        private static PropertyValidator? GetPropertyValidator(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName);
            return property is null
                ? null
                : PropertyValidator
                .CreateFromAttributes(GetAllAttributes(property).OfType<ValidationAttribute>(), propertyName);
        }

        /// <summary>
        /// Obtiene las propiedades relevantes del objeto dado.
        /// </summary>
        /// <param name="owner">El objeto que implementa IDataErrorInfo.</param>
        /// <param name="ignoreOwnerError">Indica si se debe omitir el error del propietario.</param>
        /// <returns>La colección de propiedades relevantes.</returns>
        private static IEnumerable<PropertyDescriptor> GetRelevantProperties(IDataErrorInfo owner, bool ignoreOwnerError)
        {
            var properties = TypeDescriptor.GetProperties(owner).Cast<PropertyDescriptor>();
            if (ignoreOwnerError)
            {
                var props = properties.FirstOrDefault(p => p.Name == "Error");
                if (props != null)
                {
                    properties = properties.Except(new[] { props });
                }
            }
            return properties;
        }

        /// <summary>
        /// Verifica si una propiedad tiene errores.
        /// </summary>
        /// <param name="owner">El objeto que implementa IDataErrorInfo.</param>
        /// <param name="property">La descripción de la propiedad.</param>
        /// <param name="deep">El nivel de profundidad restante para buscar errores.</param>
        /// <returns>true si se encuentra un error; en caso contrario, false.</returns>
        private static bool PropertyHasError(IDataErrorInfo owner, PropertyDescriptor property, int deep)
        {
            var errorText = owner[property.Name];
            if (!string.IsNullOrEmpty(errorText))
                return true;

            if (!TryGetPropertyValue(owner, property.Name, out var propertyValue)) return false;
            if (!(propertyValue is IDataErrorInfo dataErrorInfo)) return false;

            return HasErrors(dataErrorInfo, deep - 1);
        }

        /// <summary>
        /// Intenta obtener el valor de una propiedad del objeto especificado.
        /// </summary>
        /// <param name="owner">Objeto del cual se intentará obtener la propiedad.</param>
        /// <param name="propertyName">Nombre de la propiedad a buscar.</param>
        /// <param name="propertyValue">Valor de la propiedad encontrado (si corresponde).</param>
        /// <returns>
        /// true si la propiedad se encontró y su valor se pudo obtener; de lo contrario, false.
        /// </returns>
        private static bool TryGetPropertyValue(object owner, string propertyName, out object? propertyValue)
        {
            propertyValue = null;

            try
            {
                // Obtiene las metadatas de la propiedad
                PropertyInfo? propertyInfo = owner.GetType().GetProperty(propertyName);

                // Si no existe la propiedad o no tiene método get
                if (propertyInfo is null || !propertyInfo.CanRead)
                {
                    return false;
                }

                // Obtiene directamente el valor de la propiedad
                propertyValue = propertyInfo.GetValue(owner);
                return true;
            }
            catch (TargetParameterCountException)
            {
                // Se atrapa el error específico cuando se intenta obtener una propiedad indexada
                // sin proporcionar índices
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener la propiedad. {ex.Message}");
                return false;
            }
        }

    }
}