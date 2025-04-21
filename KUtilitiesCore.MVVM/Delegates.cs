using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Delegado invocado cuando una propiedad esta apunto de ser modificado
    /// permite alterar el comportamiento de la modificación o cancelar el cambio
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="args"></param>
    public delegate void OnPropertyChangingDelegate<TSource, TProperty>(ref OnPropertyChangingArg<TSource, TProperty> args);
}
