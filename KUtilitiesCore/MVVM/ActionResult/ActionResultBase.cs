using System.Diagnostics;

namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Clase base abstracta para resultados de acción.
    /// </summary>
    public abstract class ActionResultBase : IActionResult
    {
        #region Properties

        /// <inheritdoc/>
        public string ErrorMessage { get; protected internal set; } = string.Empty;

        /// <inheritdoc/>
        public Exception? Exception { get; protected internal set; }

        /// <inheritdoc/>
        public bool HasException => Exception != null;

        /// <inheritdoc/>
        public ActionResultStatus Status { get; protected internal set; } = ActionResultStatus.Empty;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Registra un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        protected static void LogError(string message, Exception? exception = null)
        {
            var logLevel = exception != null ? "Error" : "Warning";
            Debug.WriteLine($"{logLevel}: {message}", exception?.Message);
        }

        #endregion Methods
    }
}