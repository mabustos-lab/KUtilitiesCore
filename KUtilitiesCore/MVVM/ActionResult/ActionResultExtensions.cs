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
        /// Si el tipo de destino coincide exactamente, lo retorna.
        /// Si el resultado es un ActionResult<object>; y el tipo de destino es genérico, intenta convertirlo usando reflexión.
        /// Si no es posible, retorna null.
        /// </summary>
        /// <typeparam name="T">Tipo de resultado de acción al que se desea convertir.</typeparam>
        /// <param name="result">Instancia de resultado de acción a convertir.</param>
        /// <returns>Instancia convertida o null si no es posible.</returns>
        public static T? CastTo<T>(this IActionResult result) where T : class, IActionResult
        {
            return result switch
            {
                T exactMatch => exactMatch,
                ActionResult<object> genericObj when typeof(T).IsGenericType
                    => HandleGenericConversion<T>(genericObj),
                _ => null
            };
        }

        /// <summary>
        /// Convierte un resultado de acción a una instancia de un tipo derivado de ActionResultBase.
        /// Copia el estado, mensaje de error y excepción.
        /// Si ambos son ActionResult&lt;object&gt;, también copia el valor Result.
        /// </summary>
        /// <typeparam name="T">Tipo de destino, debe heredar de ActionResultBase y tener constructor sin parámetros.</typeparam>
        /// <param name="source">Resultado de acción fuente.</param>
        /// <returns>Instancia convertida del tipo especificado.</returns>
        public static T ConvertTo<T>(this IActionResult source) where T : ActionResultBase, new()
        {
            var result = new T
            {
                Status = source.Status,
                ErrorMessage = source.ErrorMessage,
                Exception = source.Exception
            };

            if (result is ActionResult<object> genericDest && source is ActionResult<object> genericSrc)
            {
                genericDest.Result = genericSrc.Result;
            }

            return result;
        }

        /// <summary>
        /// Realiza un cast seguro a <c>ActionResul<T>;</c>.
        /// Si el tipo coincide, lo retorna.
        /// Si es un ActionResultBase, crea un nuevo <c>ActionResul<T>;</c> copiando estado y errores.
        /// Si no es posible, retorna un resultado fallido.
        /// </summary>
        /// <typeparam name="T">Tipo de resultado genérico.</typeparam>
        /// <param name="source">Resultado de acción fuente.</param>
        /// <returns>Instancia de ActionResult&lt;T&gt; correspondiente.</returns>
        public static ActionResult<T> SafeCast<T>(this IActionResult source)
        {
            return source switch
            {
                ActionResult<T> exactMatch => exactMatch,
                ActionResultBase baseResult => new ActionResult<T>
                {
                    Status = baseResult.Status,
                    ErrorMessage = baseResult.ErrorMessage,
                    Exception = baseResult.Exception
                },
                _ => ActionResult<T>.CreateFaulted("Invalid cast operation")
            };
        }

        /// <summary>
        /// Maneja la conversión genérica de ActionResult&lt;object&gt; a ActionResult&lt;T&gt; usando reflexión.
        /// Solo convierte si el tipo del resultado coincide exactamente con el tipo genérico de destino.
        /// </summary>
        /// <typeparam name="T">Tipo de destino genérico.</typeparam>
        /// <param name="source">Instancia de ActionResult&lt;object&gt; fuente.</param>
        /// <returns>Instancia convertida o null si no es posible.</returns>
        private static T? HandleGenericConversion<T>(ActionResult<object> source) where T : class
        {
            var targetType = typeof(T).GetGenericArguments()[0];

            if (source.Result?.GetType() == targetType)
            {
                var constructor = typeof(ActionResult<>)
                    .MakeGenericType(targetType)
                    .GetMethod("CreateSuccess");

                return constructor?.Invoke(null, new[] { source.Result }) as T;
            }

            return null;
        }

        #endregion Methods
    }
}