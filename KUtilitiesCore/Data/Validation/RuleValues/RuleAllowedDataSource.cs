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

        /// <summary>
        /// Inicializa la regla con el origen de datos de consulta usado para validar
        /// la existencia de los valores.
        /// </summary>
        /// <param name="lookup">Origen de datos contra el que se validan los valores.</param>
        /// <exception cref="ArgumentNullException">Se produce cuando <paramref name="lookup"/> es null.</exception>
        public RuleAllowedDataSource(Tlookup lookup)
        {
            if (lookup == null) throw
                    new ArgumentNullException(nameof(lookup));

            _lookup = lookup;
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
        /// Valida el valor encapsulado como tupla (valor, función de consulta) contra el
        /// origen de datos, previa comprobación de tipo.
        /// </summary>
        /// <param name="value">Tupla con el valor y la función de consulta.</param>
        /// <returns><c>true</c> si el valor existe en el origen de datos.</returns>
        /// <exception cref="ArgumentNullException">Se produce cuando <paramref name="value"/> es null.</exception>
        /// <exception cref="ArgumentException">
        /// Se produce cuando <paramref name="value"/> no es la tupla esperada.
        /// </exception>
        public override bool IsAllowed(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value is not Tuple<string, Func<Tlookup, string, bool>> lookupFunc)
                throw new ArgumentException($"El parametro debe ser de tipo {typeof(Tuple<string, Func<Tlookup, string, bool>>).Name}", nameof(value));
            return IsAllowed(lookupFunc);
        }

        /// <summary>
        /// Valida la tupla (valor, función de consulta) delegando en la función de
        /// consulta proporcionada junto con el origen de datos.
        /// </summary>
        /// <param name="value">Tupla con el valor y la función de consulta.</param>
        /// <returns><c>true</c> si la función de consulta confirma que el valor es válido.</returns>
        public bool IsAllowed(Tuple<string, Func<Tlookup, string, bool>> value)
        => value.Item2(_lookup, value.Item1);

        #endregion Methods
    }
}