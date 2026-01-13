using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.ImportDefinition
{
    /// <summary>
    /// Define el contrato para una regla de validación aplicada durante el proceso de importación.
    /// Sigue el principio de Responsabilidad Única (SRP) al enfocarse solo en validar un valor.
    /// </summary>
    public interface IImportValidationRule
    {
        /// <summary>
        /// Valida un valor específico contra la regla de negocio.
        /// </summary>
        /// <param name="value">El valor ya convertido (strongly-typed) a validar.</param>
        /// <param name="fieldName">El nombre del campo o columna que se está validando (para mensajes de error).</param>
        /// <returns>Una colección de fallos de validación. Si es válida, retorna una colección vacía.</returns>
        IEnumerable<ValidationFailure> Validate(object value, string fieldName);
    }
}
