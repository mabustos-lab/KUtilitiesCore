using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Extensions
{
    public static class TypeExt
    {
        #region Methods

        /// <summary>
        /// Devuelve el argumento de tipo subyacente del tipo que acepta valores NULL especificado.
        /// </summary>
        /// <param name="type"></param>
        public static Type GetUnderlyingType(this Type type)
      => type.IsNullable() ? Nullable.GetUnderlyingType(type)! : type;

        /// <summary>
        /// Regresa true si el tipo es System.Nulable que envuelbe el tipo de valor
        /// </summary>
        /// <param name="type">El tipo para comprobar</param>
        public static bool IsNullable(this Type type)
            => type.IsGenericType
                   && (type.GetGenericTypeDefinition() == typeof(Nullable<>));

        #endregion Methods
    }
}
