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
    /// Define las características de un campo.
    /// </summary>
    public abstract class FieldDefinitionItemBase : IFieldDefinitionItem
    {
        #region Fields

        private string displayName = string.Empty;
        private Type fieldType = typeof(string);

        #endregion Fields

        #region Constructors

        public FieldDefinitionItemBase()
        { }

        public FieldDefinitionItemBase(PropertyInfo fieldProperty)
        {
            if (fieldProperty == null)
                throw new ArgumentNullException(nameof(fieldProperty));

            LoadInfo(fieldProperty);
        }

        public FieldDefinitionItemBase(string fieldName, string displayName, string sourceColumnName = "",
            string description = "", Type? fieldType = null, bool allowNull = false)
        {
            ColumnName = fieldName;
            if (string.IsNullOrEmpty(displayName))
                displayName = fieldName;

            DisplayName = displayName;
            if (string.IsNullOrEmpty(sourceColumnName))
                sourceColumnName = displayName;

            SourceColumnName = sourceColumnName;
            Description = description;
            FieldType = fieldType ?? typeof(string);
            AllowNull = allowNull;
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public bool AllowNull
        { get; protected set; }

        /// <inheritdoc/>
        [Required]
        public string ColumnName
        { get; protected set; } = string.Empty;

        /// <inheritdoc/>
        public string Description
        { get; set; } = string.Empty;

        /// <inheritdoc/>
        [Required]
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
        public Type FieldType
        {
            get => fieldType;
            protected set
            {
                fieldType = value;
                OnFieldTypeChanged();
            }
        }

        /// <inheritdoc/>
        public bool IsUnique { get; protected set; } = false;

        /// <inheritdoc/>
        [Required]
        public string SourceColumnName { get; set; }

        #endregion Properties

        #region Methods

        internal virtual void LoadInfo(PropertyInfo fieldProperty)
        {
            AllowNull = (Nullable.GetUnderlyingType(fieldProperty.PropertyType) != null);
            var underlyingType = Nullable.GetUnderlyingType(fieldProperty.PropertyType);
            var resolvedType = (AllowNull ? underlyingType : fieldProperty.PropertyType) ?? throw new InvalidOperationException($"No se pudo determinar el tipo de campo para la propiedad '{fieldProperty.Name}'.");
            FieldType = resolvedType;
            ColumnName = fieldProperty.Name;
            DisplayName = fieldProperty.DataAnnotationsDisplayName();
            Description = fieldProperty.DataAnnotationsDescription();
        }

        internal virtual void OnDisplayNameChanged()
        {
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = ColumnName;
        }

        internal abstract void OnFieldTypeChanged();

        #endregion Methods
    }
}