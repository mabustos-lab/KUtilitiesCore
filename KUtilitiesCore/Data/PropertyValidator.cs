using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    /// <summary>
    /// Clase interna responsable de realizar la validación de una propiedad utilizando <see cref="ValidationAttribute"/>.
    /// </summary>
    internal class PropertyValidator
    {
        #region Fields

        /// <summary>
        /// Colección de atributos de validación asociados a la propiedad.
        /// </summary>
        private readonly IEnumerable<ValidationAttribute> attributes;

        /// <summary>
        /// Nombre desplegado de la propiedad.
        /// </summary>
        private readonly string displayName;

        /// <summary>
        /// Nombre de la propiedad que se va a validar.
        /// </summary>
        private readonly string propertyName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor privado que inicializa los atributos, el nombre de la propiedad y el nombre desplegado.
        /// </summary>
        /// <param name="attributes">Colección de atributos de validación.</param>
        /// <param name="propertyName">Nombre de la propiedad que se va a validar.</param>
        /// <param name="displayName">Nombre desplegado de la propiedad.</param>
        private PropertyValidator(IEnumerable<ValidationAttribute> attributes, string propertyName, string displayName)
        {
            this.attributes = attributes;
            this.propertyName = propertyName;
            this.displayName = displayName;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Método estático que crea una instancia de <see cref="PropertyValidator"/> a partir de
        /// una colección de <see cref="ValidationAttribute"/>.
        /// </summary>
        /// <param name="attributes">Colección de atributos de validación.</param>
        /// <param name="propertyName">Nombre de la propiedad que se va a validar.</param>
        /// <returns>
        /// Una instancia de <see cref="PropertyValidator"/> o null si no hay atributos de validación.
        /// </returns>
        public static PropertyValidator CreateFromAttributes(IEnumerable<ValidationAttribute> attributes, string propertyName)
        {
            if (!attributes?.Any() ?? false)
                return null;

            string displayName = GetDisplayName(attributes) ?? propertyName;
            return new PropertyValidator(attributes, propertyName, displayName);
        }

        /// <summary>
        /// Obtiene el mensaje de error de validación concatenando todos los errores.
        /// </summary>
        /// <param name="value">Valor de la propiedad que se está validando.</param>
        /// <param name="instance">Instancia del objeto que contiene la propiedad.</param>
        /// <returns>Mensaje de error de validación como cadena.</returns>
        public virtual string GetValidationErrorMessage(object value, object instance)
        {
            return string.Join(" ", GetValidationErrors(value, instance));
        }

        /// <summary>
        /// Obtiene una colección de mensajes de error de validación.
        /// </summary>
        /// <param name="value">Valor de la propiedad que se está validando.</param>
        /// <param name="instance">Instancia del objeto que contiene la propiedad.</param>
        /// <returns>Colección de cadenas con los mensajes de error.</returns>
        public virtual IEnumerable<string> GetValidationErrors(object value, object instance)
        {
            return attributes.Select(
                attribute =>
                {
                    var result = attribute.GetValidationResult(value, CreateValidationContext(instance));
                    return result?.ErrorMessage;
                }
            ).Where(error => !string.IsNullOrEmpty(error));
        }

        /// <summary>
        /// Método privado que obtiene el nombre desplegado de la propiedad a partir de los atributos.
        /// </summary>
        /// <param name="attributes">Colección de atributos.</param>
        /// <returns>El nombre desplegado de la propiedad o null si no se encuentra.</returns>
        private static string GetDisplayName(IEnumerable<Attribute> attributes)
        {
            return attributes
                .OfType<DisplayAttribute>()
                .Select(a => a.GetName() ?? a.GetShortName())
                .FirstOrDefault()
                ?? attributes
                    .OfType<DisplayNameAttribute>()
                    .Select(a => a.DisplayName)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Crea un contexto de validación con el nombre de la propiedad y el nombre desplegado.
        /// </summary>
        /// <param name="instance">Instancia del objeto que se está validando.</param>
        /// <returns>Un objeto <see cref="ValidationContext"/> configurado.</returns>
        private ValidationContext CreateValidationContext(object instance)
        {
            return new ValidationContext(instance,null,null)
            {
                MemberName = propertyName,
                DisplayName = displayName.Length > 0 ? displayName : propertyName
            };
        }

        #endregion Methods
    }
}