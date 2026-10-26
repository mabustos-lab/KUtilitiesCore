namespace KUtilitiesCore.Data.Validation.RuleValues
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

        /// <summary>
        /// Indica que existe una regla activa de valores permitidos.
        /// </summary>
        public override bool HasRule => true;

        /// <summary>
        /// Fecha máxima permitida (inclusive); null si no hay límite superior.
        /// </summary>
        public DateTime? MaxDate { get; }

        /// <summary>
        /// Fecha mínima permitida (inclusive); null si no hay límite inferior.
        /// </summary>
        public DateTime? MinDate { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Construye una descripción legible del rango de fechas permitido para
        /// mostrarla en mensajes de error.
        /// </summary>
        /// <returns>Descripción del rango permitido según los límites definidos.</returns>
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

        /// <summary>
        /// Valida una fecha encapsulada en <see cref="object"/>, previa comprobación de tipo.
        /// </summary>
        /// <param name="value">Fecha a validar.</param>
        /// <returns><c>true</c> si la fecha está dentro del rango permitido.</returns>
        /// <exception cref="ArgumentException">
        /// Se produce cuando <paramref name="value"/> no es de tipo <see cref="DateTime"/>.
        /// </exception>
        public override bool IsAllowed(object value)
        {
            if (value is not DateTime dateTimeValue)
                throw new ArgumentException($"El parametro debe ser tipo {typeof(DateTime).Name}", nameof(value));
            return IsAllowed(dateTimeValue);
        }

        /// <summary>
        /// Determina si la fecha está dentro del rango permitido, comparando por fecha
        /// (sin componente horario).
        /// </summary>
        /// <param name="value">Fecha a validar.</param>
        /// <returns><c>true</c> si la fecha está dentro del rango.</returns>
        public bool IsAllowed(DateTime value)
        {
            bool minOk = !MinDate.HasValue || value.Date >= MinDate.Value.Date;
            bool maxOk = !MaxDate.HasValue || value.Date <= MaxDate.Value.Date;
            return minOk && maxOk;
        }

        #endregion Methods
    }
}