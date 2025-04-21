namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Interfaz para representar el resultado de una acción.
    /// </summary>
    public interface IActionResult
    {
        #region Properties

        /// <summary>
        /// Obtiene el mensaje de error.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Obtiene la excepción asociada.
        /// </summary>
        Exception Ex { get; }

        /// <summary>
        /// Indica si ocurrió una excepción.
        /// </summary>
        bool HasException { get; }

        /// <summary>
        /// Establece el estado de resultado
        /// </summary>
        EnumResulResult Status { get; }

        #endregion Properties
    }
}