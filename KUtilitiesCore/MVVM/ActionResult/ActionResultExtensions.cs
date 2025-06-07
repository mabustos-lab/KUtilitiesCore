namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Proporciona métodos de extensión para trabajar con resultados de acción (<see cref="IActionResult"/>).
    /// Permite realizar conversiones seguras y manipulación de resultados genéricos y no genéricos.
    /// </summary>
    public static class ActionResultExtensions
    {
        /// <summary>
        /// Realiza un cast seguro del resultado de acción al tipo especificado.
        /// Si el tipo coincide exactamente, retorna la instancia.
        /// Si es un <see cref="ActionResult{T}"/> y el tipo de destino es genérico, intenta convertirlo usando reflexión.
        /// Si no es posible, retorna null.
        /// </summary>
        /// <typeparam name="T">Tipo de destino, debe implementar <see cref="IActionResult"/>.</typeparam>
        /// <param name="result">Instancia de <see cref="IActionResult"/> a convertir.</param>
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
        /// Convierte un resultado de acción a una instancia de un tipo derivado de <see cref="ActionResultBase"/>.
        /// Copia el estado, mensaje de error y excepción.
        /// Si ambos son <see cref="ActionResult{T}"/>, también copia el valor <c>Result</c>.
        /// </summary>
        /// <typeparam name="T">Tipo de destino, debe heredar de <see cref="ActionResultBase"/> y tener constructor sin parámetros.</typeparam>
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
        /// Realiza un cast seguro a <see cref="ActionResult{T}"/>.
        /// Si el tipo coincide, lo retorna.
        /// Si es un <see cref="ActionResultBase"/>, crea un nuevo <see cref="ActionResult{T}"/> copiando estado y errores.
        /// Si no es posible, retorna un resultado fallido.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado genérico.</typeparam>
        /// <param name="source">Resultado de acción fuente.</param>
        /// <returns>Instancia de <see cref="ActionResult{T}"/> correspondiente.</returns>
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
        /// Maneja la conversión genérica de <see cref="ActionResult{T}"/> a <see cref="ActionResult{T}"/> usando reflexión.
        /// Solo convierte si el tipo del resultado coincide exactamente con el tipo genérico de destino.
        /// </summary>
        /// <typeparam name="T">Tipo de destino genérico.</typeparam>
        /// <param name="source">Instancia de <see cref="ActionResult{T}"/> fuente.</param>
        /// <returns>Instancia convertida o null si no es posible.</returns>
        private static T? HandleGenericConversion<T>(ActionResult<object> source) where T : class
        {
            var targetType = typeof(T).GetGenericArguments()[0];

            if (source.Result?.GetType() == targetType)
            {
                var constructor = typeof(ActionResult<>)
                    .MakeGenericType(targetType)
                    .GetMethod("CreateSuccess");

                return constructor?.Invoke(null, [source.Result]) as T;
            }

            return null;
        }
    }
}