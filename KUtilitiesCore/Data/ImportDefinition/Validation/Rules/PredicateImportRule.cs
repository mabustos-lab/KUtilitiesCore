using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.ImportDefinition.Validation.Rules
{
    /// <summary>
    /// Regla genérica para validaciones personalizadas usando un predicado (Func).
    /// Evita el "boxing" innecesario haciendo cast seguro internamente.
    /// </summary>
    /// <typeparam name="T">El tipo de dato esperado del valor.</typeparam>
    public class PredicateImportRule<T> : ImportValidationRuleBase
    {
        private readonly Func<T, bool> _predicate;

        public PredicateImportRule(Func<T, bool> predicate, string? errorMessage)
            : base(errorMessage ?? $"El valor de '{typeof(T).Name}' no es válido.")
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
        /// <inheritdoc/>
        public override IEnumerable<ValidationFailure> Validate(object value, string fieldName)
        {
            // Si es nulo, esta regla no aplica (usar NotNullRule para eso) o se considera válida para permitir nulos.
            if (value == null) yield break;

            ValidationFailure failure = null;

            if (value is T typedValue)
            {
                if (!_predicate(typedValue))
                {
                    failure = CreateFailure(fieldName, ErrorMessage, -1, value);
                }
            }
            else
            {
                // Seguridad de tipos: si el convertidor de ImportManager funcionó, esto no debería pasar,
                // pero si pasa, es un error de configuración de tipos.
                try
                {
                    // Intento final de conversión flexible
                    var converted = (T)Convert.ChangeType(value, typeof(T));
                    if (!_predicate(converted))
                    {
                        failure = CreateFailure(fieldName, ErrorMessage, -1, value);
                    }
                }
                catch
                {
                    failure = CreateFailure(fieldName, $"Error de tipo: Se esperaba {typeof(T).Name} pero se recibió {value.GetType().Name}.", -1, value);
                }
            }

            // Realizamos el yield fuera de los bloques try-catch
            if (failure != null)
            {
                yield return failure;
            }
        }
    }
}