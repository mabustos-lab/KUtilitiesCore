using KUtilitiesCore.Data.Converter;
using KUtilitiesCore.Data.Validation.RuleValues;
using KUtilitiesCore.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace KUtilitiesCore.Data.ImportDefinition
{
    /// <summary>
    /// Representa la definición de un campo con validación adicional.
    /// </summary>
    /// <remarks>Esta clase extiende <see cref="FieldDefinitionBase"/>.</remarks>
    public class FieldDefinition : FieldDefinitionBase, ICloneable
    {
        #region Constructors

        public FieldDefinition(PropertyInfo fieldProperty) : base(fieldProperty)
        {
        }

        public FieldDefinition(string fieldName, string displayName = "", string sourceColumnName = "",
            string description = "", Type? fieldType = null, bool allowNull = false)
            : base(fieldName, displayName, sourceColumnName, description, fieldType, allowNull)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Obtiene el convertidor de tipo asociado al campo.
        /// </summary>
        public ITypeConverter? Converter { get; private set; }

        /// <summary>
        /// Delegado para validar el tipo de dato complejo.
        /// </summary>
        public Func<object, bool>? IsValidCustom { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Valida el dato de la fuente
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsValidValueType(string value)
        {
            if (IsValidCustom != null)
                return IsValidCustom(value);
            return Converter?.CanConvert(value) ?? false;
        }

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
                AllowNull = true;
            }
        }

        /// <summary>
        /// Actualiza el nombre mapeado cuando cambia el nombre mostrado.
        /// </summary>
        internal override void OnDisplayNameChanged()
        {
            base.OnDisplayNameChanged();
            if (string.IsNullOrEmpty(SourceColumnName))
                SourceColumnName = DisplayName;
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
        public FieldDefinition Clone()
        {
            return new FieldDefinition(
                ColumnName,
                DisplayName,
                SourceColumnName,
                Description,
                FieldType,
                AllowNull)
            {
                IsValidCustom = IsValidCustom
            };
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion Methods
    }
}