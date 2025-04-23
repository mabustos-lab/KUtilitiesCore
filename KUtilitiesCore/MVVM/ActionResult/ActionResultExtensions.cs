namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Extensiones para trabajar con resultados de acción.
    /// </summary>
    public static class ActionResultExtensions
    {
        #region Methods

        /// <summary>
        /// Realiza un cast genérico seguro al resultado de acción.
        /// </summary>
        public static T CastTo<T>(this IActionResult result) where T : class, IActionResult
        { return result as T; }

        #endregion Methods
    }
}