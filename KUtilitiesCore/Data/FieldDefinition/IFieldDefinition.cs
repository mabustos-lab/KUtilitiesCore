using KUtilitiesCore.Data.Validation.RuleValues;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.FieldDefinition
{
    /// <summary>
    /// Define las características de un campo.
    /// </summary>
    public interface IFieldDefinition
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
        /// Nombre del campo para mostrar al usuario.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Nombre interno o técnico del campo.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Tipo de dato esperado para el campo.
        /// </summary>
        Type FieldType { get; }

        ///// <summary>
        ///// Obtiene un valor que indica si el campo debe ser único.
        ///// </summary>
        //bool IsUnique { get; set; }
    }
}