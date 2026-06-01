using System;
using System.Threading;
using KUtilitiesCore.MVVM.Command;

namespace KUtilitiesCore.MVVM.Command.Binder
{
    /// <summary>
    /// Clase base para los enlazares de comandos. Proporciona la infraestructura común
    /// para gestionar la suscripción, desuscripción y actualización del estado del objetivo.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto que se está enlazando.</typeparam>
    internal abstract class CommandBinderBase<T> : IDisposable where T : class
    {
        protected readonly IViewModelCommand Command;
        protected readonly T TargetObject;
        protected readonly Action<bool>? TargetStatus;
        protected readonly SynchronizationContext? CapturedSyncContext;
        protected bool IsDisposed;

        protected CommandBinderBase(T targetObject, Action<bool>? targetStatus, IViewModelCommand command)
        {
            TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            TargetStatus = targetStatus;
            CapturedSyncContext = SynchronizationContext.Current;

            Command.CanExecuteChanged += UpdateTargetStatus;
            Subscribe();
        }

        /// <summary>
        /// Implementa la suscripción específica al evento o mecanismo de disparo del comando.
        /// </summary>
        protected abstract void Subscribe();

        /// <summary>
        /// Implementa la desuscripción específica para evitar fugas de memoria.
        /// </summary>
        protected abstract void Unsubscribe();

        protected bool GetCanExecuteLogic()
        {
            object? parameter = Command.IsParametrizedCommand ? Command.GetViewModelParameter() : null;
            return Command.CanExecute(parameter);
        }

        private void UpdateTargetStatus(object? sender, EventArgs e)
        {
            if (CapturedSyncContext != null && CapturedSyncContext != SynchronizationContext.Current)
            {
                CapturedSyncContext.Post(state => TargetStatus?.Invoke((bool)state!), GetCanExecuteLogic());
            }
            else
            {
                TargetStatus?.Invoke(GetCanExecuteLogic());
            }
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            Unsubscribe();
            Command.CanExecuteChanged -= UpdateTargetStatus;
            IsDisposed = true;
        }
    }
}
