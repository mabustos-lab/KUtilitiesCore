using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Validation.Core
{
    /// <summary>
    /// Interfaz para un validador de un tipo específico.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto a validar.</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Valida la instancia especificada.
        /// </summary>
        /// <param name="instance">La instancia a validar.</param>
        /// <returns>Un ValidationResult con los resultados.</returns>
        ValidationResult Validate(T instance);

        /// <summary>
        /// Valida la instancia especificada usando un contexto.
        /// </summary>
        /// <param name="context">El contexto de validación.</param>
        /// <returns>Un ValidationResult con los resultados.</returns>
        ValidationResult Validate(ValidationContext<T> context);
    }
}
