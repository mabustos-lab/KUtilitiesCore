using KUtilitiesCore.Data.Converter;
using KUtilitiesCore.Data.ImportDefinition.Validation;
using KUtilitiesCore.Data.Validation.RuleValues;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.ImportDefinition
{
    /// <summary>
    /// Define las características de un campo.
    /// </summary>
    public interface IFieldDefinitionItem
    {

        /// <summary>
        /// Indica si se permiten valores nulos para este campo.
        /// </summary>
        bool AllowNull { get; }

        /// <summary>
        /// Descripción del campo en pantalla
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Nombre del campo para mostrar al usuario o al crear una nueva plantilla.
        /// </summary>
        [Required]
        string DisplayName { get; }

        /// <summary>
        /// Nombre de la columna en el archivo origen de datos (ej. Excel, CSV).
        /// </summary>
        [Required]
        string SourceColumnName { get; set; }

        /// <summary>
        /// Nombre interno o técnico del campo.
        /// </summary>
        [Required]
        string FieldName { get; }

        /// <summary>
        /// Tipo de dato esperado para el campo.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Obtiene un valor que indica si el campo debe ser único.
        /// </summary>
        bool IsUnique { get;  }
        /// <summary>
        /// Establece el valor default al importar un valor nulo
        /// </summary>
        object DefaultValue { get; set; }
        /// <summary>
        /// Obtiene el convertidor de tipo asociado al campo.
        /// </summary>
        ITypeConverter TypeConverter { get; }
        /// <summary>
        /// Colección de reglas de validación de negocio que se ejecutarán tras la conversión de tipo.
        /// </summary>
        List<IImportValidationRule> ValidationRules { get; }

        /// <summary>
        /// Método fluido para configurar reglas de validación.
        /// </summary>
        IFieldDefinitionItem WithRules(Action<ImportRuleBuilder> ruleConfig);
    }
}