using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Messaging
{
    /// <summary>
    /// Almacena una acción sin causar una referencia dura al propietario de la acción.
    /// Permite que el propietario sea recolectado por el Garbage Collector en cualquier momento.
    /// </summary>
    internal class WeakAction
    {
        #region Campos

        private readonly Action action;
        private WeakReference reference;

        #endregion

        #region Constructores

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="WeakAction"/>.
        /// </summary>
        /// <param name="target">El objeto que posee la acción.</param>
        /// <param name="action">La acción asociada a este WeakAction.</param>
        public WeakAction(object target, Action action)
        {
            reference = new WeakReference(target);
            this.action = action;
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Obtiene la acción asociada a esta instancia.
        /// </summary>
        public Action Action => action;

        /// <summary>
        /// Obtiene si el propietario de la acción sigue vivo.
        /// </summary>
        public bool IsAlive => reference != null && reference.IsAlive;

        /// <summary>
        /// Obtiene el propietario de la acción.
        /// </summary>
        public object Target => reference?.Target;

        #endregion

        #region Métodos

        /// <summary>
        /// Ejecuta la acción si el propietario sigue vivo.
        /// </summary>
        public void Execute()
        {
            if (action == null || !IsAlive) return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw;
            }
        }

        /// <summary>
        /// Marca la instancia para su eliminación.
        /// </summary>
        public void MarkForDeletion()
        {
            reference = null;
        }

        #endregion
    }
    /// <summary>
    /// Almacena una acción genérica sin causar una referencia dura al propietario de la acción.
    /// Implementa <see cref="IExecuteWithObject"/> para permitir ejecutar la acción con parámetros.
    /// </summary>
    /// <typeparam name="T">Tipo del parámetro de la acción.</typeparam>
    internal class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        #region Campos

        private readonly Action<T> action;

        #endregion

        #region Constructores

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="WeakAction{T}}"/>.
        /// </summary>
        /// <param name="target">El objeto que posee la acción.</param>
        /// <param name="action">La acción genérica asociada a esta instancia.</param>
        public WeakAction(object target, Action<T> action) : base(target, null)
        {
            this.action = action;
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Obtiene la acción genérica asociada a esta instancia.
        /// </summary>
        public new Action<T> Action => action;

        #endregion

        #region Métodos

        /// <summary>
        /// Ejecuta la acción con el valor predeterminado del tipo T.
        /// </summary>
        public new void Execute()
        {
            if (action == null || !base.IsAlive) return;
            action(default(T));
        }

        /// <summary>
        /// Ejecuta la acción con el parámetro especificado.
        /// </summary>
        /// <param name="parameter">Parámetro a pasar a la acción.</param>
        public void Execute(T parameter)
        {
            if (action == null || !base.IsAlive) return;
            action(parameter);
        }

        /// <summary>
        /// Ejecuta la acción con un parámetro de objeto, que será casteado a T.
        /// Implementa <see cref="IExecuteWithObject.ExecuteWithObject(object)"/>.
        /// </summary>
        /// <param name="parameter">Parámetro a pasar a la acción.</param>
        public void ExecuteWithObject(object parameter)
        {
            if (parameter == null || action == null || !base.IsAlive) return;

            try
            {
                Execute((T)parameter);
            }
            catch (InvalidCastException ex)
            {
                // Log the exception if needed
                throw new ArgumentException("Invalid parameter type", ex);
            }
        }

        #endregion
    }
}
