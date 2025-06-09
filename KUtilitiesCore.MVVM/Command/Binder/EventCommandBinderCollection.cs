using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Command.Binder
{
    /// <summary>
    /// Colección que administra el ciclo de vida de múltiples enlaces entre eventos de objetos y comandos de un ViewModel.
    /// Permite enlazar eventos de controles de UI a comandos del ViewModel de forma segura y desechable.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// Tipo del ViewModel que implementa <see cref="ISupportCommands"/> y expone los comandos a enlazar.
    /// </typeparam>
    public sealed class EventCommandBinderCollection<TViewModel> : IDisposable
        where TViewModel : class, ISupportCommands
    {

        /// <summary>
        /// Lista interna de enlaces activos entre eventos y comandos.
        /// </summary>
        private readonly List<IDisposable> _binders = new();

        /// <summary>
        /// Instancia del ViewModel asociado a la colección de enlaces.
        /// </summary>
        private readonly TViewModel _viewModel;

        private bool disposedValue;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="EventCommandBinderCollection{TViewModel}"/>.
        /// </summary>
        /// <param name="viewModel">Instancia del ViewModel que expone los comandos a enlazar.</param>
        /// <exception cref="ArgumentNullException">Si <paramref name="viewModel"/> es null.</exception>
        public EventCommandBinderCollection(TViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        /// Enlaza de forma segura y desechable un evento de un objeto a un <see cref="IViewModelCommand"/>.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto que expone el evento.</typeparam>
        /// <param name="targetObject">El objeto que expone el evento (ej. un control de UI).</param>
        /// <param name="eventExpression">
        /// Expresión lambda que identifica el evento a enlazar (ej. <c>c =&gt; c.Click</c>).
        /// </param>
        /// <param name="targetStatus">
        /// Acción que establece el estado del objeto según si el comando puede ejecutarse.
        /// </param>
        /// <param name="command">El comando que se ejecutará cuando el evento se dispare.</param>
        public void BindCommand<T>(T targetObject, Expression<Func<T, Delegate>> eventExpression,
            Action<bool> targetStatus, IViewModelCommand command)
            where T : class
        {
            _binders.Add(new EventCommandBinder<T>(targetObject, eventExpression, targetStatus, command));
        }

        /// <summary>
        /// Enlaza de forma segura y desechable un evento de un objeto a un comando del ViewModel, usando una expresión de método sin parámetros.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto que expone el evento.</typeparam>
        /// <param name="targetObject">El objeto que expone el evento (ej. un control de UI).</param>
        /// <param name="eventExpression">
        /// Expresión lambda que identifica el evento a enlazar (ej. <c>c =&gt; c.Click</c>).
        /// </param>
        /// <param name="targetStatus">
        /// Acción que establece el estado del objeto según si el comando puede ejecutarse.
        /// </param>
        /// <param name="executeExpression">
        /// Expresión que representa el método a ejecutar en el ViewModel.
        /// </param>
        public void BindCommand<T>(T targetObject, Expression<Func<T, Delegate>> eventExpression,
            Action<bool> targetStatus, Expression<Action<TViewModel>> executeExpression)
            where T : class
        {
            BindCommand(
                targetObject,
                eventExpression,
                targetStatus,
                RelayCommand<TViewModel>.Create(_viewModel, executeExpression));
        }

        /// <summary>
        /// Enlaza de forma segura y desechable un evento de un objeto a un comando del ViewModel, usando una expresión de método con parámetro.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto que expone el evento.</typeparam>
        /// <typeparam name="TParam">Tipo del parámetro del comando.</typeparam>
        /// <param name="targetObject">El objeto que expone el evento (ej. un control de UI).</param>
        /// <param name="eventExpression">
        /// Expresión lambda que identifica el evento a enlazar (ej. <c>c =&gt; c.Click</c>).
        /// </param>
        /// <param name="targetStatus">
        /// Acción que establece el estado del objeto según si el comando puede ejecutarse.
        /// </param>
        /// <param name="executeExpression">
        /// Expresión que representa el método a ejecutar en el ViewModel con parámetro.
        /// </param>
        /// <param name="parameterPropertyExpression">
        /// Expresión que representa la propiedad del parámetro en el ViewModel.
        /// </param>
        public void BindCommand<T, TParam>(T targetObject, Expression<Func<T, Delegate>> eventExpression,
            Action<bool> targetStatus, Expression<Action<TViewModel, TParam?>> executeExpression,
            Expression<Func<TViewModel, TParam>> parameterPropertyExpression)
            where T : class
        {
            BindCommand(
                targetObject,
                eventExpression,
                targetStatus,
                RelayCommand<TViewModel, TParam>.Create(_viewModel, executeExpression, parameterPropertyExpression));
        }

        /// <summary>
        /// Libera todos los recursos utilizados por la colección y los enlaces internos.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos administrados y no administrados utilizados por la colección.
        /// </summary>
        /// <param name="disposing">Indica si se deben liberar los recursos administrados.</param>
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var binder in _binders)
                    {
                        binder.Dispose();
                    }
                    _binders.Clear();
                }
                disposedValue = true;
            }
        }
    }
}