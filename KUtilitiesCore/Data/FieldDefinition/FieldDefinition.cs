using KUtilitiesCore.Data.Converter;
using KUtilitiesCore.Data.Validation.RuleValues;
using KUtilitiesCore.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace KUtilitiesCore.Data.FieldDefinition
{
    /// <summary>
    /// Representa la definición de un campo con validación adicional.
    /// </summary>
    public class FieldDefinition : FieldDefinitionBase, IFieldValidation
    {
        #region Properties

        /// <inheritdoc/>
        public IRuleValue AllowedValueDefinition { get; set; } = new EmptyRuleValue();

        /// <inheritdoc/>
        public ITypeConverter Converter { get; private set; }

        /// <inheritdoc/>
        public bool IsRequerided { get; set; }

        /// <inheritdoc/>
        public bool IsUnique { get; set; }

        /// <inheritdoc/>
        public string MappedName
        { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Carga la información del campo a partir de la propiedad proporcionada.
        /// </summary>
        /// <param name="fieldProperty">La propiedad que define el campo.</param>
        internal override void LoadInfo(PropertyInfo fieldProperty)
        {
            base.LoadInfo(fieldProperty);

            // Verifica si la propiedad tiene el atributo Key para marcarla como única.
            if (fieldProperty.GetCustomAttribute<KeyAttribute>() != null)
            {
                IsUnique = true;
            }

            // Verifica si la propiedad tiene el atributo Required para marcarla como requerida.
            if (fieldProperty.GetCustomAttribute<RequiredAttribute>() != null)
            {
                IsRequerided = true;
            }
        }

        internal override void OnDisplayNameChanged()
        {
            base.OnDisplayNameChanged();
            MappedName = FieldName;
        }

        internal override void OnFieldTypeChanged()
        {
            if (FieldType != null)
            {
                // Resuelve el convertidor de tipo para el tipo de campo actual.
                Converter = TypeConverterFactory.Provider.Resolve(FieldType);
            }
        }

        #endregion Methods
    }
}