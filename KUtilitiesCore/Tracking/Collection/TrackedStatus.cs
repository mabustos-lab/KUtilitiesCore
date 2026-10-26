using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Tracking.Collection
{
    /// <summary>
    /// Define los estados de los elementos de la colección
    /// </summary>
    public enum TrackedStatus
    {
        /// <summary>El elemento no tiene cambios desde su carga o inserción.</summary>
        UnModified,
        /// <summary>El elemento fue agregado a la colección.</summary>
        Added,
        /// <summary>El elemento fue modificado tras su carga o inserción.</summary>
        Modified,
        /// <summary>El elemento fue marcado para eliminación de la colección.</summary>
        Removed
    }
}