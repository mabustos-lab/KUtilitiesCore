using KUtilitiesCore.Data.Validation.RuleValues;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.FieldDefinition
{
    /// <summary>
    /// Representa la definición de un campo con validación adicional.
    /// </summary>
    public interface IFieldValidation
    {
        /// <summary>
        /// Establece el nombre del campo en la fuente de datos, por default es el Nombre mostrado en pantalla
        /// </summary>
        string SourceColumnName { get; set; }
        /// <summary>
        /// Indica si el campo debe ser único.
        /// </summary>
        bool IsUnique { get; set; }
                
    }
}