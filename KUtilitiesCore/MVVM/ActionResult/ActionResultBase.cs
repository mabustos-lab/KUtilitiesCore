using KUtilitiesCore.Diagnostics.Logger;

namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Clase base abstracta para resultados de acción.
    /// </summary>
    public abstract class ActionResultBase : IActionResult
    {
        #region Constructors

        protected ActionResultBase()
        { Status = EnumResulResult.Empty; }

        #endregion Constructors

        #region Properties

        /// <inheritdoc/>
        public string ErrorMessage { get; protected set; }

        /// <inheritdoc/>
        public Exception Ex { get; protected set; }

        /// <inheritdoc/>
        public bool HasException => Ex != null;

        /// <inheritdoc/>
        public EnumResulResult Status { get; protected set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Registra un mensaje de error.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        protected static void LogError(string message, Exception exception = null)
        {
            if (exception != null)
                LogFactory.Service.LogError(message, exception);
            else
                LogFactory.Service.LogWarning(message);
        }

        #endregion Methods
    }
}