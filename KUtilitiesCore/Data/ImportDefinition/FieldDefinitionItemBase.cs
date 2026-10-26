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
    /// Define las características de un campo.
    /// </summary>
    public abstract class FieldDefinitionItemBase : IFieldDefinitionItem
    {
        #region Fields

        private string displayName = string.Empty;
        private Type fieldType = typeof(string);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una definición de campo vacía, con tipo <see cref="string"/> por defecto.
        /// </summary>
        /// <remarks>
        /// Pensado para escenarios de configuración diferida donde las propiedades se
        /// asignan después de la construcción.
        /// </remarks>
        public FieldDefinitionItemBase()
        { }

        /// <summary>
        /// Inicializa la definición de campo a partir de los metadatos de una propiedad.
        /// </summary>
        /// <param name="fieldProperty">Propiedad de la que se extraen nombre, tipo y atributos.</param>
        /// <exception cref="ArgumentNullException">Se lanza si <paramref name="fieldProperty"/> es <c>null</c>.</exception>
        public FieldDefinitionItemBase(PropertyInfo fieldProperty)
        {
            if (fieldProperty == null)
                throw new ArgumentNullException(nameof(fieldProperty));

            LoadInfo(fieldProperty);
        }

        /// <summary>
        /// Inicializa la definición de campo con valores explícitos.
        /// </summary>
        /// <param name="fieldName">Nombre interno o técnico del campo.</param>
        /// <param name="displayName">Nombre para mostrar; si está vacío se usa <paramref name="fieldName"/>.</param>
        /// <param name="sourceColumnName">Columna de origen en la fuente de datos; si está vacía se usa <paramref name="displayName"/>.</param>
        /// <param name="description">Descripción del campo en pantalla.</param>
        /// <param name="fieldType">Tipo de dato esperado; si es <c>null</c> se usa <see cref="string"/>.</param>
        /// <param name="allowNull">Indica si el campo acepta valores nulos.</param>
        public FieldDefinitionItemBase(string fieldName, string displayName, string sourceColumnName = "",
            string description = "", Type? fieldType = null, bool allowNull = false)
        {
            FieldName = fieldName;
            if (string.IsNullOrEmpty(displayName))
                displayName = fieldName;

            DisplayName = displayName;
            if (string.IsNullOrEmpty(sourceColumnName))
                sourceColumnName = displayName;

            SourceColumnName = sourceColumnName;
            Description = description;
            TargetType = fieldType ?? typeof(string);
            AllowNull = allowNull;
        }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public bool AllowNull
        { get; protected set; }

        /// <inheritdoc/>
        [Required]
        public string FieldName
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
        public Type TargetType
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
        public string SourceColumnName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public object? DefaultValue { get; set; }

        /// <inheritdoc/>
        public ITypeConverter? TypeConverter { get; internal set; }
        /// <inheritdoc/>
        public abstract List<IImportValidationRule> ValidationRules { get; }

        /// <inheritdoc/>
        public abstract IFieldDefinitionItem WithRules(Action<ImportRuleBuilder> ruleConfig);

        #endregion Properties

        #region Methods

        internal virtual void LoadInfo(PropertyInfo fieldProperty)
        {
            AllowNull = (Nullable.GetUnderlyingType(fieldProperty.PropertyType) != null);
            var underlyingType = Nullable.GetUnderlyingType(fieldProperty.PropertyType);
            var resolvedType = (AllowNull ? underlyingType : fieldProperty.PropertyType) ?? throw new InvalidOperationException($"No se pudo determinar el tipo de campo para la propiedad '{fieldProperty.Name}'.");
            TargetType = resolvedType;
            FieldName = fieldProperty.Name;
            DisplayName = fieldProperty.DataAnnotationsDisplayName();
            Description = fieldProperty.DataAnnotationsDescription();
        }

        internal virtual void OnDisplayNameChanged()
        {
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = FieldName;
        }
        /// <inheritdoc/>
        internal abstract void OnFieldTypeChanged();

        #endregion Methods
    }
}