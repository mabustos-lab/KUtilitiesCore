namespace KUtilitiesCore.Validation.RuleValues
{
    /// <summary>
    /// Implementación de IAllowedValue para una lista de cadenas permitidas.
    /// </summary>
    public class RuleAllowedStringValues : BaseRuleValue, IRuleAllowedValue<string>
    {
        #region Fields

        private readonly HashSet<string> _allowedValues;
        private readonly StringComparison _comparisonType;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Crea una instancia con una lista de valores permitidos.
        /// </summary>
        /// <param name="allowedValues">La colección de cadenas permitidas.</param>
        /// <param name="comparisonType">El tipo de comparación a usar (por defecto: OrdinalIgnoreCase).</param>
        public RuleAllowedStringValues(IEnumerable<string> allowedValues, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            if (allowedValues == null) throw new ArgumentNullException(nameof(allowedValues));

            _comparisonType = comparisonType;
            // Usamos HashSet para búsquedas eficientes O(1)
            _allowedValues = new HashSet<string>(allowedValues.Where(v => v != null), GetStringComparer(comparisonType));
        }

        #endregion Constructors

        #region Properties

        public override bool HasRule => true;

        #endregion Properties

        #region Methods

        public override string GetAllowedDescription()
        {
            return $"uno de [{string.Join(", ", _allowedValues.Select(v => $"'{v}'"))}]";
        }

        public override bool IsAllowed(object value)
        {
            if (value is not string strValue)
                throw new ArgumentException($"El parametro debe ser tipo {typeof(string).Name}", nameof(value));
            return IsAllowed(strValue);
        }

        public bool IsAllowed(string value)
        {
            // Considerar si null debe ser permitido explícitamente o manejado por otra regla (NotNull)
            if (value == null) return false; // O podría depender de un flag 'allowNull'
            return _allowedValues.Contains(value);
        }

        private static IEqualityComparer<string> GetStringComparer(StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture: return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase: return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture: return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase: return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.Ordinal: return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase: return StringComparer.OrdinalIgnoreCase;
                default: throw new ArgumentOutOfRangeException(nameof(comparisonType));
            }
        }

        #endregion Methods
    }
}