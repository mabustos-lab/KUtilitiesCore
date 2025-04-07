namespace KUtilitiesCore.Data.Validation.RuleValues
{
    /// <summary>
    /// Interfaz genérica para definir conjuntos de valores permitidos.
    /// </summary>
    /// <typeparam name="T">El tipo del valor a verificar.</typeparam>
    public interface IRuleAllowedValue<T> : IRuleValue
    {
        #region Methods

        /// <summary>
        /// Verifica si el valor proporcionado está dentro del conjunto permitido.
        /// </summary>
        /// <param name="value">El valor a verificar.</param>
        /// <returns>True si el valor está permitido, False en caso contrario.</returns>
        bool IsAllowed(T value);

        #endregion Methods
    }
}