namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Resultado de una acción.
    /// </summary>
    public class ActionResult : ActionResultBase
    {
        #region Propiedades estáticas

        /// <summary>
        /// Resultado generado cuando la acción se canceló.
        /// </summary>
        public static readonly ActionResult CancelResult =
            new ActionResult { Status = EnumResulResult.Canceled };

        public static readonly ActionResult EmptyResult =
            new ActionResult();

        /// <summary>
        /// Resultado generado cuando la acción se completó correctamente.
        /// </summary>
        public static readonly ActionResult SuccessResult =
            new ActionResult { Status = EnumResulResult.Succes };

        #endregion Propiedades estáticas

        #region Métodosakiipiakii

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        /// <param name="logError">Indica si se debe registrar el error.</param>
        public static ActionResult CreateFaultedResult(string message, Exception exception = null, bool logError = true)
        {
            var result = new ActionResult { Status = EnumResulResult.Faulted, ErrorMessage = message, Ex = exception };

            if (logError)
                LogError(message, exception);

            return result;
        }

        #endregion Métodosakiipiakii
    }

    /// <summary>
    /// Resultado genérico de una acción.
    /// </summary>
    /// <typeparam name="TResult">Tipo del resultado.</typeparam>
    public class ActionResult<TResult> : ActionResultBase
    {
        #region Propiedades estáticas

        public static readonly ActionResult<TResult> EmptyResult =
            new ActionResult<TResult>();

        #endregion Propiedades estáticas

        #region Propiedades

        /// <summary>
        /// Obtiene el resultado genérico.
        /// </summary>
        public TResult Result { get; set; }

        #endregion Propiedades

        #region Métodos estáticos

        /// <summary>
        /// Crea un resultado cancelado.
        /// </summary>
        public static ActionResult<TResult> CreateCancelResult()
        { return new ActionResult<TResult> { Status = EnumResulResult.Canceled }; }

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        public static ActionResult<TResult> CreateFaultedResult(string message, Exception exception = null)
        {
            var result = new ActionResult<TResult>
            {
                Status = EnumResulResult.Faulted,
                ErrorMessage = message,
                Ex = exception
            };

            if (exception != null)
                LogError(message, exception);

            return result;
        }

        /// <summary>
        /// Crea un resultado fallido basado en otro resultado fallido.
        /// </summary>
        /// <param name="source">Resultado fuente.</param>
        public static ActionResult<TResult> CreateFaultedResult(IActionResult source)
        {
            return new ActionResult<TResult>
            {
                Status = EnumResulResult.Faulted,
                ErrorMessage = source.ErrorMessage,
                Ex = source.Ex
            };
        }

        /// <summary>
        /// Crea un resultado exitoso con un valor.
        /// </summary>
        /// <param name="result">Valor resultante.</param>
        public static ActionResult<TResult> CreateSuccessResult(TResult result)
        { return new ActionResult<TResult> { Status = EnumResulResult.Succes, Result = result }; }

        #endregion Métodos estáticos
    }
}