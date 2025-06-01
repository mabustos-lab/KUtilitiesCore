using KUtilitiesCore.Extensions;

namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary>
    /// Implementación de IAllowedValue para una lista de cadenas permitidas.
    /// </summary>
    public class RuleAllowedStringValues : BaseRuleValue, IRuleAllowedValue<string>
    {
        #region Fields

        private readonly HashSet<string> _allowedValues;

        private readonly StringComparison _comparisonType;
        private readonly bool _ignoreAcents;

        public bool AllowNull { get; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Crea una instancia con una lista de valores permitidos.
        /// </summary>
        /// <param name="allowedValues">La colección de cadenas permitidas.</param>
        /// <param name="comparisonType">El tipo de comparación a usar (por defecto: OrdinalIgnoreCase).</param>
        /// <param name="ignoreAcents">Si es True normaliza las cadenas de Texto ignorando los acentos.</param>
        public RuleAllowedStringValues(IEnumerable<string> allowedValues,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase, bool allowNull = false, bool ignoreAcents = false)
        {
            if (allowedValues == null) throw new ArgumentNullException(nameof(allowedValues));
            AllowNull = allowNull;
            _comparisonType = comparisonType;
            _ignoreAcents = ignoreAcents;
            // Usamos HashSet para búsquedas eficientes O(1)
            _allowedValues = new HashSet<string>(allowedValues.Where(v => v != null)
                .Select(s => _ignoreAcents ? s.ToNormalized() : s)
                , GetStringComparer(_comparisonType));
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
            if (string.IsNullOrEmpty(value))
                return AllowNull;
            return _allowedValues.Contains(_ignoreAcents ? value.ToNormalized() : value);
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