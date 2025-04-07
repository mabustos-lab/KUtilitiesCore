namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary> Implementación genérica de IAllowedValue para un rango numérico. </summary>
    /// <typeparam name="TNum">El tipo numérico (debe implementar IComparable<TNum>).</typeparam>
    public class RuleAllowedNumberRange<TNum> : BaseRuleValue, IRuleAllowedValue<TNum> where TNum : struct, IComparable<TNum>
    {
        #region Constructors

        /// <summary>
        /// Crea una instancia con un rango numérico permitido. Los límites son inclusivos.
        /// </summary>
        /// <param name="minValue">
        /// El valor mínimo permitido (inclusive). Null si no hay límite inferior.
        /// </param>
        /// <param name="maxValue">
        /// El valor máximo permitido (inclusive). Null si no hay límite superior.
        /// </param>
        public RuleAllowedNumberRange(TNum? minValue, TNum? maxValue)
        {
            if (minValue.HasValue && maxValue.HasValue && minValue.Value.CompareTo(maxValue.Value) > 0)
            {
                throw new ArgumentException("MinValue no puede ser mayor que MaxValue.");
            }
            MinValue = minValue;
            MaxValue = maxValue;
        }

        #endregion Constructors

        #region Properties

        public override bool HasRule => true;
        public TNum? MaxValue { get; }
        public TNum? MinValue { get; }

        #endregion Properties

        #region Methods

        public override string GetAllowedDescription()
        {
            string minStr = MinValue?.ToString() ?? "-inf";
            string maxStr = MaxValue?.ToString() ?? "+inf";

            if (MinValue.HasValue && MaxValue.HasValue)
                return $"entre {minStr} y {maxStr} (inclusivo)";
            if (MinValue.HasValue)
                return $"mayor o igual a {minStr}";
            if (MaxValue.HasValue)
                return $"menor o igual a {maxStr}";
            return "cualquier número";
        }

        public override bool IsAllowed(object value)
        {
            if (value is not TNum tnumValue)
                throw new ArgumentException($"El parametro debe ser tipo {typeof(TNum).Name}", nameof(value));
            return IsAllowed(tnumValue);
        }

        public bool IsAllowed(TNum value)
        {
            bool minOk = !MinValue.HasValue || value.CompareTo(MinValue.Value) >= 0;
            bool maxOk = !MaxValue.HasValue || value.CompareTo(MaxValue.Value) <= 0;
            return minOk && maxOk;
        }

        #endregion Methods
    }
}