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
    /// Define las características de un campo.
    /// </summary>
    public abstract class FieldDefinitionBase : IFieldDefinition
    {
        #region Fields

        private string displayName;
        private Type fieldType;

        #endregion Fields

        #region Constructors

        public FieldDefinitionBase()
        { }

        public FieldDefinitionBase(PropertyInfo fieldProperty)
        {
            if (fieldProperty == null)
                throw new ArgumentNullException(nameof(fieldProperty));
            LoadInfo(fieldProperty);
        }

        public FieldDefinitionBase(string fieldName, string displayName, string description, Type fieldType, bool allowNull)
        {
            FieldName = fieldName;
            DisplayName = displayName;
            Description = description;
            FieldType = fieldType;
            AllowNull = allowNull;
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public bool AllowNull
        { get; private set; }

        /// <inheritdoc/>
        public string Description
        { get; set; }

        /// <inheritdoc/>
        public string DisplayName
        {
            get => displayName;
            set
            {
                displayName = value;
                OnDisplayNameChanged();
            }
        }

        /// <inheritdoc/>
        public string FieldName
        { get; private set; }

        /// <inheritdoc/>
        public Type FieldType
        {
            get => fieldType;
            private set
            {
                fieldType = value;
                OnFieldTypeChanged();
            }
        }

        #endregion Properties

        #region Methods

        internal virtual void LoadInfo(PropertyInfo fieldProperty)
        {
            AllowNull = (Nullable.GetUnderlyingType(fieldProperty.PropertyType) != null);
            FieldType = AllowNull ? Nullable.GetUnderlyingType(fieldProperty.PropertyType) : fieldProperty.PropertyType;
            FieldName = fieldProperty.Name;
            DisplayName = fieldProperty.DataAnnotationsDisplayName();
            Description = fieldProperty.DataAnnotationsDescription();
        }

        internal virtual void OnDisplayNameChanged()
        {
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = FieldName;
        }

        internal abstract void OnFieldTypeChanged();

        #endregion Methods
    }
}