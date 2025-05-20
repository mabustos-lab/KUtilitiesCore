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
        /// <remarks>
        /// Esta clase extiende <see cref="FieldDefinitionBase"/> e implementa <see cref="IFieldValidation"/> para proporcionar
        /// validaciones adicionales como unicidad, obligatoriedad y reglas de valores permitidos.
        /// </remarks>
        public class FieldDefinition : FieldDefinitionBase, IFieldValidation
        {
            #region Properties

            /// <summary>
            /// Define las reglas de valores permitidos para el campo.
            /// </summary>
            public IRuleValue AllowedValueDefinition { get; set; } = new EmptyRuleValue();

            /// <summary>
            /// Obtiene el convertidor de tipo asociado al campo.
            /// </summary>
            public ITypeConverter? Converter { get; private set; }

            /// <summary>
            /// Indica si el campo es requerido.
            /// </summary>
            public bool IsRequerided { get; set; }

            /// <summary>
            /// Indica si el campo debe ser único.
            /// </summary>
            public bool IsUnique { get; set; }

            /// <summary>
            /// Establece el nombre del campo en la fuente de datos, por default es el Nombre mostrado en pantalla.
            /// </summary>
            public string MappedName { get; set; } = string.Empty;

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

            /// <summary>
            /// Actualiza el nombre mapeado cuando cambia el nombre mostrado.
            /// </summary>
            internal override void OnDisplayNameChanged()
            {
                base.OnDisplayNameChanged();
                MappedName = FieldName;
            }

            /// <summary>
            /// Actualiza el convertidor de tipo cuando cambia el tipo de campo.
            /// </summary>
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