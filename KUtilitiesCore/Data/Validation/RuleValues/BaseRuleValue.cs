namespace KUtilitiesCore.Data.Validation.RuleValues
{
    public abstract class BaseRuleValue : IRuleValue
    {
        #region Properties

        public virtual bool HasRule => false;

        #endregion Properties

        #region Methods

        public virtual string GetAllowedDescription()
        {
            return string.Empty;
        }

        public virtual bool IsAllowed(object value)
        {
            return true;
        }

        #endregion Methods
    }
}