using KUtilitiesCore.Data;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace KUtilitiesCore.MVVM
{
    public abstract class ViewModelHelperBase
        : IViewModelHelper, ISupportParameter, ISupportParentViewModel,
        IViewModelDocumentContent, IViewModelDataErrorInfo, INotifyPropertyChanged
    {
        #region Fields

        //private readonly object errorDictionaryLock = new();
        private IViewModelDocumentOwner documentOwner;
        private string error;
        private ConcurrentDictionary<string, string> errorMessages;
        private bool hasValidationErrors;
        private bool isLoading;
        private object parentViewModel;

        #endregion Fields

        #region Events

        /// <summary>
        /// Indica cuando se llama el metodo OnDestroy para comunicar que se termino el Modelo
        /// </summary>
        public event EventHandler DestroyViewModel;

        /// <inheritdoc/>
        public event EventHandler ParentViewModelChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public IViewModelDocumentOwner DocumentOwner
        {
            get => documentOwner;
            set
            {
                documentOwner = value;
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

        /// <summary>
        /// Delegado que se usa para actualizar el estado en pantalla
        /// </summary>
        [Display(AutoGenerateField = false)]
        public Action<string> MessageStatusLoading { get; set; }

        /// <inheritdoc/>
        [Display(AutoGenerateField = false)]
        public object Parameter { get; set; }

        [Display(AutoGenerateField = false)]
        object ISupportParentViewModel.ParentViewModel
        {
            get => parentViewModel;
            set => this.SetVMValue(ref parentViewModel, value, null, OnParentViewModelChangedBase);
        }

        /// <inheritdoc/>
        public abstract object Title { get; }

        #endregion Properties

        #region Indexers

        /// <inheritdoc/>
        string IDataErrorInfo.this[string columnName] => GetErrorMessage(columnName);

        #endregion Indexers

        #region Methods

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.ClearErrors()
        {
            hasValidationErrors = false;
            error = string.Empty;
            if (errorMessages != null && errorMessages.Count > 0)
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

        /// <summary>
        /// Es usado para inicializar el modelo si se requiere
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

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.SetError(string propertyName, string errorMessage)
        {
            if (errorMessages == null)
                errorMessages = new ConcurrentDictionary<string, string>();
            errorMessages[propertyName] = errorMessage;
        }

        /// <inheritdoc/>
        void IViewModelDataErrorInfo.SetError(string errMessage)
        {
            error = errMessage;
        }

        /// <summary>
        /// Metodo invocado cuando alguna propiedad ha cambiado de valor
        /// </summary>
        [Display(AutoGenerateField = false)]
        public virtual void Update()
        {
            HasErrors();
            UpdateCommands();
        }

        internal virtual string GetErrorMessage(string columnName)
         => GetErrorMessageCore(columnName);

        /// <summary>
        /// Envia un mensaje para el usuario de estado cuando IsLoading=True
        /// </summary>
        /// <param name="message"></param>
        [Display(AutoGenerateField = false)]
        protected internal void SendMessageStatus(string message)
        {
            if (IsLoading)
                MessageStatusLoading?.Invoke(message ?? "");
        }

        protected virtual void Close()
        {
            if (DocumentOwner != null)
                DocumentOwner.Close(this);
        }

        /// <summary>
        /// Establece un mensaje de error en tiempo de ejecución para una propiedad específica del objeto.
        /// </summary>
        /// <param name="sender">
        /// Instancia del objeto que implementa <see cref="IViewModelDataErrorInfo"/> y que genera
        /// el error.
        /// </param>
        /// <param name="columnName">El nombre de la propiedad que contiene el error.</param>
        protected virtual void CustomColumnMessageError(IViewModelDataErrorInfo sender, string columnName)
        { }

        /// <summary>
        /// Establece un mensaje de error general para el objeto.
        /// </summary>
        /// <param name="sender">
        /// Instancia del objeto que implementa <see cref="IViewModelDataErrorInfo"/> y que genera
        /// el error.
        /// </param>
        protected virtual void CustomMessageError(IViewModelDataErrorInfo sender)
        { }

        /// <inheritdoc/>
        protected virtual void OnClose(CancelEventArgs e)
        { }

        /// <inheritdoc/>
        protected abstract void OnDestroy();

        /// <summary>
        /// Es invocado cuando cambia de estado la propiedad <see cref="IsLoading"/>
        /// </summary>
        protected virtual void OnIsLoadingChanged()
        {
            SendMessageStatus("Trabajando...");
        }

        /// <summary>
        /// Es invocado cuando cambia el objeto padre del ViewModel
        /// </summary>
        protected virtual void OnParentViewModelChanged()
        {
        }

        /// <summary>
        /// Actualiza el estado de los comandos del viewmodel
        /// </summary>
        protected abstract void UpdateCommands();

        private string GetErrorMessageCore(string columnName)
        {
            if (errorMessages == null)
                errorMessages = new ConcurrentDictionary<string, string>();
            string ret = string.Empty;
            if (!errorMessages.ContainsKey(columnName))
            {
                ret = this.GetErrorText(columnName);
                if (string.IsNullOrEmpty(ret))
                    CustomColumnMessageError(this, columnName);
            }
            else
                ret = errorMessages?[columnName];
            if (!string.IsNullOrEmpty(ret))
                errorMessages[columnName] = ret;
            return ret;
        }

        private void HasErrors()
        {
            ((IViewModelDataErrorInfo)(this)).ClearErrors();
            CustomMessageError(this);
            hasValidationErrors = DataErrorInfoExt.HasErrors(this)
                || !string.IsNullOrEmpty(error)
                || (errorMessages != null && errorMessages.Count > 0);
        }

        /// <summary>
        /// Es invocado cuando cambia el objeto padre del ViewModel
        /// </summary>
        private void OnParentViewModelChangedBase()
        {
            OnParentViewModelChanged();
            ParentViewModelChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}