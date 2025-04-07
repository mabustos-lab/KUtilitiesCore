using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Tracking
{
    /// <summary>
    /// Atributo que permite a una propiedad ser ignorada para el seguimiento
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ChangeTrackerIgnoreAttribute : Attribute
    { }
}