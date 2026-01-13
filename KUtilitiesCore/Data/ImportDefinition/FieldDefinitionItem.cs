using KUtilitiesCore.Data.Converter;
using KUtilitiesCore.Data.ImportDefinition.Validation;
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
    /// <remarks>Esta clase extiende <see cref="FieldDefinitionItemBase"/>.</remarks>
    public class FieldDefinitionItem : FieldDefinitionItemBase, ICloneable
    {
        #region Fields

        private readonly List<IImportValidationRule> validationRules = [];

        #endregion Fields

        #region Constructors

        public FieldDefinitionItem(PropertyInfo fieldProperty) : base(fieldProperty)
        {
        }

        public FieldDefinitionItem(string fieldName, string displayName = "", string sourceColumnName = "",
            string description = "", Type? fieldType = null, bool allowNull = false)
            : base(fieldName, displayName, sourceColumnName, description, fieldType, allowNull)
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Delegado para validar el tipo de dato complejo.
        /// </summary>
        public Func<object, bool>? IsValidCustom { get; set; }

        /// <inheritdoc/>
        public override List<IImportValidationRule> ValidationRules => validationRules;

        #endregion Properties

        #region Methods

        public FieldDefinitionItem Clone()
        {
            var clone = new FieldDefinitionItem(FieldName, DisplayName, SourceColumnName, Description, TargetType, AllowNull)
            {
                IsValidCustom = IsValidCustom,
                DefaultValue = DefaultValue
            };
            clone.ValidationRules.AddRange(validationRules);
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Valida el dato de la fuente
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsValidValueType(string value)
        {
            if (IsValidCustom != null)
                return IsValidCustom(value);
            return TypeConverter?.CanConvert(value) ?? false;
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
            if (TargetType != null)
            {
                // Resuelve el convertidor de tipo para el tipo de campo actual.
                TypeConverter = TypeConverterFactory.Provider.Resolve(TargetType);
            }
        }
        /// <inheritdoc/>
        public override IFieldDefinitionItem WithRules(Action<ImportRuleBuilder> ruleConfig)
        {
            if (ruleConfig == null) throw new ArgumentNullException(nameof(ruleConfig));

            var builder = new ImportRuleBuilder(this);
            ruleConfig(builder);

            return this;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{FieldName} ({TargetType.Name})";
        }
        #endregion Methods
    }
}