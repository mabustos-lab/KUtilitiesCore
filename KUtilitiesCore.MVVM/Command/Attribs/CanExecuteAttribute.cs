using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Command.Attribs
{
    /// <summary>
    /// Atributo para especificar el nombre del método CanExecute asociado a una acción de un comando.
    /// Se utiliza cuando el nombre del método CanExecute no sigue la convención "Can[NombreDelMetodoExecute]".
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CanExecuteAttribute : Attribute
    {
        /// <summary>
        /// Obtiene el nombre del método CanExecute.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="CanExecuteAttribute"/>.
        /// </summary>
        /// <param name="methodName">El nombre del método a invocar para la lógica CanExecute.</param>
        public CanExecuteAttribute(string methodName)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }
    }
}
