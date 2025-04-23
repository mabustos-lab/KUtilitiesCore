using System;
using System.Linq;

namespace KUtilitiesCore.MVVM.ActionResult
{
    /// <summary>
    /// Representa los posibles estados de un resultado de acción.
    /// </summary>
    public enum EnumResulResult
    {
        /// <summary>
        /// Indica que el resultado está vacío o no se ha establecido.
        /// </summary>
        Empty,

        /// <summary>
        /// Indica que la acción se completó con éxito.
        /// </summary>
        Succes,

        /// <summary>
        /// Indica que la acción falló.
        /// </summary>
        Faulted,

        /// <summary>
        /// Indica que la acción fue cancelada.
        /// </summary>
        Canceled
    }
}
