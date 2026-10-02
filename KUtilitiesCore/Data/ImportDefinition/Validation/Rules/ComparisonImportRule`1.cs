using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Linq;

namespace KUtilitiesCore.Data.ImportDefinition.Validation.Rules
{
    /// <summary>
    /// Regla para validar rangos numéricos (Mayor que, Menor que, etc.)
    /// </summary>
    /// <typeparam name="T">Tipo comparable (int, decimal, DateTime, etc.)</typeparam>
    public class ComparisonImportRule<T> : ImportValidationRuleBase where T : IComparable
    {
        private readonly T _valueToCompare;
        private readonly ComparisonOperator _op;
        /// <summary>
        /// Operadores de comparación soportados por la regla.
        /// </summary>
        public enum ComparisonOperator
        {
            /// <summary>El valor debe ser mayor que el comparador.</summary>
            GreaterThan,
            /// <summary>El valor debe ser menor que el comparador.</summary>
            LessThan,
            /// <summary>El valor debe ser mayor o igual que el comparador.</summary>
            GreaterThanOrEqual,
            /// <summary>El valor debe ser menor o igual que el comparador.</summary>
            LessThanOrEqual,
            /// <summary>El valor debe ser exactamente igual al comparador.</summary>
            Equal,
            /// <summary>El valor debe ser distinto del comparador.</summary>
            NotEqual
        }

        /// <summary>
        /// Inicializa la regla con el valor de comparación y el operador a aplicar.
        /// </summary>
        /// <param name="valueToCompare">Valor contra el que se compara el dato importado.</param>
        /// <param name="op">Operador de comparación a aplicar.</param>
        /// <param name="errorMessage">Mensaje de error; si es <c>null</c> se genera uno descriptivo.</param>
        public ComparisonImportRule(T valueToCompare, ComparisonOperator op, string? errorMessage = null) : base(errorMessage)
        {
            _valueToCompare = valueToCompare;
            _op = op;
        }
        /// <inheritdoc/>
        public override IEnumerable<ValidationFailure> Validate(object? value, string fieldName)
        {
            if (value == null) yield break;

            if (value is IComparable comparableVal)
            {
                // Aseguramos que comparamos tipos compatibles
                int comparisonResult;
                try
                {
                    comparisonResult = comparableVal.CompareTo(_valueToCompare);
                }
                catch (ArgumentException)
                {
                    // Intento de conversión para comparación (ej. int vs long)
                    var converted = (T)Convert.ChangeType(value, typeof(T));
                    comparisonResult = converted.CompareTo(_valueToCompare);
                }

                bool isValid = _op switch
                {
                    ComparisonOperator.GreaterThan => comparisonResult > 0,
                    ComparisonOperator.LessThan => comparisonResult < 0,
                    ComparisonOperator.GreaterThanOrEqual => comparisonResult >= 0,
                    ComparisonOperator.LessThanOrEqual => comparisonResult <= 0,
                    ComparisonOperator.Equal => comparisonResult == 0,
                    ComparisonOperator.NotEqual => comparisonResult != 0,
                    _ => true
                };

                if (!isValid)
                {
                    yield return CreateFailure(fieldName, ErrorMessage ?? $"El valor '{value}' no cumple la condición {_op} {_valueToCompare}.", -1, value);
                }
            }
        }
    }
}