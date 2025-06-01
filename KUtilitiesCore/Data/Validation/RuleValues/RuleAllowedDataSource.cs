namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary>
    /// Implementación de IAllowedValue para una para un origen de datos.
    /// </summary>
    public class RuleAllowedDataSource<Tlookup> : BaseRuleValue, IRuleAllowedValue<Tuple<string, Func<Tlookup, string, bool>>>
    {
        #region Fields

        private readonly Tlookup _lookup;

        #endregion Fields

        #region Constructors

        public RuleAllowedDataSource(Tlookup lookup)
        {
            if (lookup == null) throw
                    new ArgumentNullException(nameof(lookup));

            _lookup = lookup;
        }

        #endregion Constructors

        #region Properties

        public override bool HasRule => true;

        #endregion Properties

        #region Methods

        public override bool IsAllowed(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value is not Tuple<string, Func<Tlookup, string, bool>> lookupFunc)
                throw new ArgumentException($"El parametro debe ser de tipo {typeof(Tuple<string, Func<Tlookup, string, bool>>).Name}", nameof(value));
            return IsAllowed(lookupFunc);
        }

        public bool IsAllowed(Tuple<string, Func<Tlookup, string, bool>> value)
        => value.Item2(_lookup, value.Item1);

        #endregion Methods
    }
}