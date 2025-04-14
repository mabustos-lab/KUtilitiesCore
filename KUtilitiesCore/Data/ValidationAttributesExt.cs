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
        /// Valida las propiedades de un objeto usando <see cref="ValidationAttribute"/>.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <param name="source">Objeto que se desea validar y que contiene marcas de <see cref="ValidationAttribute"/>.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si el objeto es válido; de lo contrario, false.</returns>
        public static bool TryValidateObject<TSource>(this TSource source, List<ValidationResult> validationResults)
        {
            var context = new ValidationContext(source);
            return Validator.TryValidateObject(source, context, validationResults, true);
        }
        /// <summary>
        /// Valida una propiedad específica de un objeto usando attributes de validación.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad a validar.</typeparam>
        /// <param name="source">Objeto que contiene la propiedad a validar.</param>
        /// <param name="propertyExpression">Expresión que representa la propiedad a validar.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si la propiedad es válida; de lo contrario, false.</returns>
        public static bool ValidateProperty<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyExpression, List<ValidationResult> validationResults)
        {
            var propertyName = propertyExpression.GetFullPathProperty();
            return source.ValidateProperty<TSource, TProperty>(propertyName, validationResults);
        }
        /// <summary>
        /// Valida una propiedad específica de un objeto usando attributes de validación.
        /// </summary>
        /// <typeparam name="TSource">Tipo del objeto a validar.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad a validar.</typeparam>
        /// <param name="source">Objeto que contiene la propiedad a validar.</param>
        /// <param name="propertyName">Nombre de la propiedad a validar.</param>
        /// <param name="validationResults">Lista para almacenar los resultados de validación.</param>
        /// <returns>true si la propiedad es válida; de lo contrario, false.</returns>
        public static bool ValidateProperty<TSource, TProperty>(this TSource source,
            string propertyName, List<ValidationResult> validationResults)
        {
            var context = new ValidationContext(source)
            {
                MemberName = propertyName
            };
            var value = source.GetPropertyValueOfPath(propertyName);
            return Validator.TryValidateProperty(value, context, validationResults);
        }
    }
}
