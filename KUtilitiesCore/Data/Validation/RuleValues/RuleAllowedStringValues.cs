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

        /// <summary>
        /// Indica si el valor validado puede ser null o una cadena vacía.
        /// </summary>
        public bool AllowNull { get; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Crea una instancia con una lista de valores permitidos.
        /// </summary>
        /// <param name="allowedValues">La colección de cadenas permitidas.</param>
        /// <param name="comparisonType">El tipo de comparación a usar (por defecto: OrdinalIgnoreCase).</param>
        /// <param name="allowNull">Si es True permite que el valor validado sea Null.</param>
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

        /// <summary>
        /// Indica que existe una regla activa de valores permitidos.
        /// </summary>
        public override bool HasRule => true;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Construye una descripción legible de la lista de valores permitidos para
        /// mostrarla en mensajes de error.
        /// </summary>
        /// <returns>Descripción de los valores permitidos.</returns>
        public override string GetAllowedDescription()
        {
            return $"uno de [{string.Join(", ", _allowedValues.Select(v => $"'{v}'"))}]";
        }

        /// <summary>
        /// Valida una cadena encapsulada en <see cref="object"/>, previa comprobación de tipo.
        /// </summary>
        /// <param name="value">Cadena a validar.</param>
        /// <returns><c>true</c> si la cadena pertenece al conjunto permitido.</returns>
        /// <exception cref="ArgumentException">
        /// Se produce cuando <paramref name="value"/> no es de tipo <see cref="string"/>.
        /// </exception>
        public override bool IsAllowed(object value)
        {
            if (value is not string strValue)
                throw new ArgumentException($"El parametro debe ser tipo {typeof(string).Name}", nameof(value));
            return IsAllowed(strValue);
        }

        /// <summary>
        /// Determina si la cadena pertenece al conjunto permitido, aplicando la
        /// normalización de acentos y el comparador configurados.
        /// </summary>
        /// <param name="value">Cadena a validar.</param>
        /// <returns>
        /// <c>true</c> si la cadena está permitida; las cadenas vacías se rigen por
        /// <see cref="AllowNull"/>.
        /// </returns>
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