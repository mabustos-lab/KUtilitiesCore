namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Interfaz para representar el resultado de una acción.
    /// </summary>
    public interface IActionResult
    {

        /// <summary>
        /// Obtiene el mensaje de error.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Obtiene la excepción asociada.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// Indica si ocurrió una excepción.
        /// </summary>
        bool HasException { get; }

        /// <summary>
        /// Establece el estado de resultado
        /// </summary>
        ActionResultStatus Status { get; }

    }
}