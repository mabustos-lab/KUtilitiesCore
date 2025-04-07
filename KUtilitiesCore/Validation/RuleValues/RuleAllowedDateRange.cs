namespace KUtilitiesCore.Validation.RuleValues
{
    /// <summary>
    /// Implementación de IAllowedValue para un rango de fechas.
    /// </summary>
    public class RuleAllowedDateRange : BaseRuleValue, IRuleAllowedValue<DateTime>
    {
        #region Constructors

        /// <summary>
        /// Crea una instancia con un rango de fechas permitido. Los límites son inclusivos.
        /// </summary>
        /// <param name="minDate">
        /// La fecha mínima permitida (inclusive). Null si no hay límite inferior.
        /// </param>
        /// <param name="maxDate">
        /// La fecha máxima permitida (inclusive). Null si no hay límite superior.
        /// </param>
        public RuleAllowedDateRange(DateTime? minDate, DateTime? maxDate)
        {
            if (minDate.HasValue && maxDate.HasValue && minDate.Value > maxDate.Value)
            {
                throw new ArgumentException("MinDate no puede ser posterior a MaxDate.");
            }
            MinDate = minDate;
            MaxDate = maxDate;
        }

        #endregion Constructors

        #region Properties

        public override bool HasRule => true;
        public DateTime? MaxDate { get; }
        public DateTime? MinDate { get; }

        #endregion Properties

        #region Methods

        public override string GetAllowedDescription()
        {
            if (MinDate.HasValue && MaxDate.HasValue)
                return $"entre {MinDate.Value:d} y {MaxDate.Value:d}";
            if (MinDate.HasValue)
                return $"posterior o igual a {MinDate.Value:d}";
            if (MaxDate.HasValue)
                return $"anterior o igual a {MaxDate.Value:d}";
            return "cualquier fecha";
        }

        public override bool IsAllowed(object value)
        {
            if (value is not DateTime dateTimeValue)
                throw new ArgumentException($"El parametro debe ser tipo {typeof(DateTime).Name}", nameof(value));
            return IsAllowed(dateTimeValue);
        }

        public bool IsAllowed(DateTime value)
        {
            bool minOk = !MinDate.HasValue || value.Date >= MinDate.Value.Date;
            bool maxOk = !MaxDate.HasValue || value.Date <= MaxDate.Value.Date;
            return minOk && maxOk;
        }

        #endregion Methods
    }
}