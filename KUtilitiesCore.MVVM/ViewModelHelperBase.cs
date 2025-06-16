using KUtilitiesCore.Data;
using KUtilitiesCore.MVVM.Command;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.MVVM
{
    /// <summary>
    /// Clase base abstracta para ViewModels en aplicaciones MVVM. Gestiona validación de datos,
    /// estado de carga, comunicación con el modelo del documento y registro de comandos.
    /// </summary>
    public abstract class ViewModelHelperBase
        : IViewModelHelper, ISupportParameter, ISupportCommands, ISupportParentViewModel,
        IViewModelDocumentContent, IViewModelDataErrorInfo, INotifyPropertyChanged
    {
        private readonly List<RelayCommandBase> _registeredCommands = [];
        private IViewModelDocumentOwner? documentOwner;
        private string error = string.Empty;
        private ConcurrentDictionary<string, string> errorMessages = [];
        private bool hasValidationErrors;
        private bool isLoading;
        private object? parentViewModel;

        /// <summary>
        /// Se dispara cuando cambia el estado de <see cref="IsLoading"/>.
        /// </summary>
        public event EventHandler<string>? MessageStatusLoadingChanged;

        /// <inheritdoc/>
        public event EventHandler? ParentViewModelChanged;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _title = string.Empty;

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public IViewModelDocumentOwner DocumentOwner
        {
            get => documentOwner ?? throw new InvalidOperationException("DocumentOwner is not initialized.");
            set
            {
                documentOwner = value ?? throw new ArgumentNullException(nameof(value));
                if (documentOwner != null)
                    documentOwner.Content = this;
            }
        }

        /// <inheritdoc/>
        string IDataErrorInfo.Error => error;

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public bool HasValidationErrors
         => !string.IsNullOrEmpty(error) || (errorMessages?.Count > 0) || hasValidationErrors;

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public bool IsLoading
        {
            get { return isLoading; }
            set { this.SetVMValue(ref isLoading, value, null, OnIsLoadingChanged); }
        }

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public object? Parameter { get; set; }

        [Display(AutoGenerateField = false)]
        object? ISupportParentViewModel.ParentViewModel
        {
            get => parentViewModel;
            set => this.SetVMValue(ref parentViewModel, value, null, OnParentViewModelChangedBase);
        }

#pragma warning disable CS8601 // Non-nullable field is uninitialized. Agregado por que persiste el mensaje

        /// <inheritdoc/>
        public string Title
        {
            get => _title;
            set { this.SetVMValue(ref _title, value ?? string.Empty); }
        }

#pragma warning restore CS8601

        /// <inheritdoc/>
        string IDataErrorInfo.this[string columnName] => GetErrorMessage(columnName);

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.ClearErrors()
        {
            hasValidationErrors = false;
            error = string.Empty;
            if (errorMessages != null && !errorMessages.IsEmpty)
                errorMessages.Clear();
        }

        void IViewModelDocumentContent.OnClose(CancelEventArgs e)
        {
            OnClose(e);
        }

        void IViewModelDocumentContent.OnDestroy()
        {
            OnDestroy();
        }

        /// <inheritdoc/>
        public abstract void OnDestroy();

        /// <summary>
        /// Inicializa el modelo si es necesario. Implementar en clases derivadas.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public abstract void OnLoaded();

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(propertyName);
            Update();
        }

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public virtual void RaisePropertyChanging([CallerMemberName] string propertyName = "")
        {
        }

        /// <summary>
        /// Registra un comando para que su estado CanExecute sea re-evaluado automáticamente cuando
        /// una propiedad del ViewModel cambie.
        /// </summary>
        /// <typeparam name="TCommand">Tipo de comando a registrar.</typeparam>
        /// <param name="command">El comando a registrar.</param>
        void ISupportCommands.RegisterCommand<TCommand>(TCommand command)
        {
            _registeredCommands.Add(command);
        }

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.SetError(string propertyName, string errorMessage)
        {
            errorMessages ??= new ConcurrentDictionary<string, string>();
            errorMessages[propertyName] = errorMessage;
        }

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.SetError(string errorMessage)
        {
            error = errorMessage;
        }

        /// <summary>
        /// Actualiza el estado de validación y comandos del ViewModel.
        /// </summary>
        [Display(AutoGenerateField = false)]
        public virtual void Update()
        {
            HasErrors();
            ((ISupportCommands)this).UpdateRegisteredCommands();
            UpdateCommands();
        }

        /// <summary>
        /// Itera sobre los comandos registrados y notifica que su estado de ejecución puede haber cambiado.
        /// </summary>
        void ISupportCommands.UpdateRegisteredCommands()
        {
            foreach (var command in _registeredCommands)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Obtiene el mensaje de error asociado a una propiedad.
        /// </summary>
        /// <param name="columnName">Nombre de la propiedad.</param>
        /// <returns>Mensaje de error, si existe.</returns>
        internal virtual string GetErrorMessage(string columnName)
                 => GetErrorMessageCore(columnName);

        /// <summary>
        /// Envía un mensaje de estado cuando <see cref="IsLoading"/> es verdadero.
        /// </summary>
        /// <param name="message">Mensaje a mostrar.</param>
        [Display(AutoGenerateField = false)]
        protected internal void SendMessageStatus(string message)
        {
            if (IsLoading)
                MessageStatusLoadingChanged?.Invoke(this, message ?? "");
        }

        /// <summary>
        /// Cierra el documento actual usando el <see cref="DocumentOwner"/>.
        /// </summary>
        protected virtual void Close()
        {
            DocumentOwner?.Close(this);
        }

        /// <summary>
        /// Permite establecer un mensaje de error personalizado para una propiedad específica.
        /// Implementar en clases derivadas si se requiere lógica especial.
        /// </summary>
        /// <param name="sender">Instancia que genera el error.</param>
        /// <param name="columnName">Nombre de la propiedad con error.</param>
        protected virtual void CustomColumnMessageError(IViewModelDataErrorInfo sender, string columnName)
        { }

        /// <summary>
        /// Permite establecer un mensaje de error general personalizado. Implementar en clases
        /// derivadas si se requiere lógica especial.
        /// </summary>
        /// <param name="sender">Instancia que genera el error.</param>
        protected virtual void CustomMessageError(IViewModelDataErrorInfo sender)
        { }

        /// <summary>
        /// Ejecuta lógica personalizada cuando se inicia la operación de cierre.
        /// </summary>
        /// <param name="e">Datos del evento de cierre, permite cancelar la operación.</param>
        protected abstract void OnClose(CancelEventArgs e);

        /// <summary>
        /// Se invoca cuando cambia el estado de <see cref="IsLoading"/>.
        /// </summary>
        protected virtual void OnIsLoadingChanged()
        {
            SendMessageStatus("Trabajando...");
        }

        /// <summary>
        /// Se invoca cuando cambia el objeto padre del ViewModel.
        /// </summary>
        protected virtual void OnParentViewModelChanged()
        {
        }

        /// <summary>
        /// Actualiza el estado de los comandos definidos en el ViewModel. Implementar en clases derivadas.
        /// </summary>
        protected abstract void UpdateCommands();

        /// <summary>
        /// Obtiene el mensaje de error para una propiedad, consultando primero el diccionario de errores.
        /// </summary>
        /// <param name="columnName">Nombre de la propiedad.</param>
        /// <returns>Mensaje de error, si existe.</returns>
        private string GetErrorMessageCore(string columnName)
        {
            errorMessages ??= new ConcurrentDictionary<string, string>();
            string ret;
            if (!errorMessages.ContainsKey(columnName))
            {
                ret = this.GetErrorText(columnName);
                if (string.IsNullOrEmpty(ret))
                    CustomColumnMessageError(this, columnName);
            }
            else
                ret = errorMessages?[columnName] ?? string.Empty;
            if (!string.IsNullOrEmpty(ret))
                errorMessages![columnName] = ret;
            return ret;
        }

        /// <summary>
        /// Determina si existen errores de validación en el ViewModel.
        /// </summary>
        private void HasErrors()
        {
            ((IViewModelDataErrorInfo)(this)).ClearErrors();
            CustomMessageError(this);
            hasValidationErrors = DataErrorInfoExt.HasErrors(this)
                || !string.IsNullOrEmpty(error)
                || (errorMessages != null && !errorMessages.IsEmpty);
        }

        /// <summary>
        /// Lógica interna para notificar el cambio del ViewModel padre.
        /// </summary>
        private void OnParentViewModelChangedBase()
        {
            OnParentViewModelChanged();
            ParentViewModelChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Notifica a la UI que una propiedad ha cambiado.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}