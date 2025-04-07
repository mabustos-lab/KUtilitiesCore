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
        None,
        Added,
        Modified,
        Removed
    }
}