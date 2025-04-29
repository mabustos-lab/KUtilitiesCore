using System;
using System.Linq;

namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Implementación del RuleBuilder fluido.
    /// </summary>
    internal class RuleBuilder<T, TProperty> : IRuleBuilderInitial<T, TProperty>
    {
        internal readonly PropertyRule<T, TProperty> Rule;

        internal RuleBuilder(PropertyRule<T, TProperty> rule)
        {
            Rule = rule;
        }

        // Método interno para añadir validadores específicos
        internal void AddValidator(IPropertyValidator<T, TProperty> validator)
        {
            Rule.AddValidator(validator);
        }
    }
}
