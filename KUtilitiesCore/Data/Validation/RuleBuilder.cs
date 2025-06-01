namespace KUtilitiesCore.Data.Validation
{
    /// <summary>
    /// Implementación del RuleBuilder fluido.
    /// </summary>
    internal class RuleBuilder<T, TProperty> : IRuleBuilderInitial<T, TProperty>
    {
        #region Fields

        internal readonly PropertyRule<T, TProperty> Rule;

        #endregion Fields

        #region Constructors

        internal RuleBuilder(PropertyRule<T, TProperty> rule)
        {
            Rule = rule;
        }

        #endregion Constructors

        #region Methods

        // Método interno para añadir validadores específicos
        internal void AddValidator(IPropertyValidator<T, TProperty> validator)
        {
            Rule.AddValidator(validator);
        }

        #endregion Methods
    }
}