using KUtilitiesCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    public static class ValidationAttributesExt
    {
        /// <summary>
        /// Valida las propiedades de un objeto usando los atributos de validación.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <param name="source">Objeto a validar.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si el objeto es válido; de lo contrario, false.</returns>
        public static bool ValidateObject<TSource>(this TSource source, List<ValidationResult> validationResults)
            where TSource : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var context = new ValidationContext(source);
            return Validator.TryValidateObject(source, context, validationResults, true);
        }

        /// <summary>
        /// Valida una propiedad específica de un objeto usando una expresión lambda.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad a validar.</typeparam>
        /// <param name="source">Objeto que contiene la propiedad a validar.</param>
        /// <param name="propertyExpression">Expresión que representa la propiedad a validar.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si la propiedad es válida; de lo contrario, false.</returns>
        public static bool ValidateProperty<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyExpression,
            List<ValidationResult> validationResults)
            where TSource : class
        {
            string propertyName = propertyExpression.GetFullPathProperty();
            return ValidateProperty(source, propertyName, validationResults);
        }

        /// <summary>
        /// Valida una propiedad específica de un objeto usando su nombre.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <param name="source">Objeto que contiene la propiedad a validar.</param>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si la propiedad es válida; de lo contrario, false.</returns>
        public static bool ValidateProperty<TSource>(this TSource source,
            string propertyName,
            List<ValidationResult> validationResults)
            where TSource : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var context = new ValidationContext(source) { MemberName = propertyName };
            var propertyValue = GetPropertyValue(source, propertyName);
            return Validator.TryValidateProperty(propertyValue, context, validationResults);
        }

        private static object? GetPropertyValue<TSource>(TSource source, string propertyName)
        {
            var property = typeof(TSource).GetProperty(propertyName);
            if (property is null)
            {
                throw new ArgumentException($"Propiedad {propertyName} no existe en {typeof(TSource)}", nameof(propertyName));
            }

            return property.GetValue(source);
        }
    }
}
