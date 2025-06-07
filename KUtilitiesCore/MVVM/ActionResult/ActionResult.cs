namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Resultado de una acción.
    /// </summary>
    public class ActionResult : ActionResultBase
    {

        /// <summary>
        /// Resultado generado cuando la acción se canceló.
        /// </summary>
        public static ActionResult CancelResult =>
            new()
            { Status = ActionResultStatus.Canceled };
        /// <summary>
        /// Obtiene una instancia Vacia
        /// </summary>
        public static ActionResult Empty =>
            new();

        /// <summary>
        /// Resultado generado cuando la acción se completó correctamente.
        /// </summary>
        public static ActionResult SuccessResult =>
            new()
            { Status = ActionResultStatus.Succes };

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        /// <param name="logError">Indica si se debe registrar el error.</param>
        public static ActionResult CreateFaultedResult(string message, Exception? exception = null, bool logError = true)
        {
            var result = new ActionResult { Status = ActionResultStatus.Faulted, ErrorMessage = message, Exception = exception };

            if (logError)
                LogError(message, exception);

            return result;
        }

    }

    /// <summary>
    /// Resultado genérico de una acción.
    /// </summary>
    /// <typeparam name="TResult">Tipo del resultado.</typeparam>
    public class ActionResult<TResult> : ActionResultBase
    {

        /// <summary>
        /// Crea un resultado cancelado.
        /// </summary>
        public static ActionResult<TResult> Canceled
            => new()
            { Status = ActionResultStatus.Canceled };

        public static ActionResult<TResult> Empty =>
                    new();

        /// <summary>
        /// Obtiene el resultado genérico.
        /// </summary>
        public TResult? Result { get; set; }

        /// <summary>
        /// Crea un resultado fallido.
        /// </summary>
        /// <param name="message">Mensaje del error.</param>
        /// <param name="exception">Excepción asociada (opcional).</param>
        public static ActionResult<TResult> CreateFaulted(string message, Exception? exception = null)
        {
            var result = new ActionResult<TResult>
            {
                Status = ActionResultStatus.Faulted,
                ErrorMessage = message,
                Exception = exception
            };

            if (exception != null)
                LogError(message, exception);

            return result;
        }

        /// <summary>
        /// Crea un resultado fallido basado en otro resultado fallido.
        /// </summary>
        /// <param name="source">Resultado fuente.</param>
        public static ActionResult<TResult> CreateFaulted(IActionResult source)
        {
            return new ActionResult<TResult>
            {
                Status = ActionResultStatus.Faulted,
                ErrorMessage = source.ErrorMessage,
                Exception = source.Exception
            };
        }

        /// <summary>
        /// Crea un resultado exitoso con un valor.
        /// </summary>
        /// <param name="result">Valor resultante.</param>
        public static ActionResult<TResult> CreateSuccess(TResult? result) => new()
        {
            Status = ActionResultStatus.Succes,
            Result = result
        };

        /// <summary>
        /// Operador implícito para conversión automática de valores exitosos
        /// </summary>
        public static implicit operator ActionResult<TResult>(TResult? value)
        {
            return CreateSuccess(value);
        }

        /// <summary>
        /// Operador implícito para conversión automática de excepciones
        /// </summary>
        public static implicit operator ActionResult<TResult>(Exception exception)
        {
            return CreateFaulted(exception.Message, exception);
        }

    }
}