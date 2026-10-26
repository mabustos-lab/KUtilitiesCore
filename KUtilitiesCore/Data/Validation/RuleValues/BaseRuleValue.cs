namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary>
    /// Implementación base de <see cref="IRuleValue"/> con el comportamiento por defecto
    /// de "sin regla activa": todo valor está permitido y la descripción es vacía.
    /// Las reglas concretas sobrescriben los miembros que apliquen.
    /// </summary>
    public abstract class BaseRuleValue : IRuleValue
    {
        #region Properties

        /// <summary>
        /// Indica si existe una regla activa que restrinja los valores permitidos.
        /// </summary>
        public virtual bool HasRule => false;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Devuelve una descripción legible de los valores permitidos; vacía cuando
        /// no hay regla activa.
        /// </summary>
        /// <returns>Descripción de los valores permitidos.</returns>
        public virtual string GetAllowedDescription()
        {
            return string.Empty;
        }

        /// <summary>
        /// Determina si el valor está permitido. Sin regla activa, todo valor se
        /// considera permitido.
        /// </summary>
        /// <param name="value">Valor a verificar.</param>
        /// <returns><c>true</c> si el valor está permitido; <c>false</c> en caso contrario.</returns>
        public virtual bool IsAllowed(object value)
        {
            return true;
        }

        #endregion Methods
    }
}