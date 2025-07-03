using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    /// <summary>
    /// Clase base para la validación de los campos de un objeto
    /// </summary>
    public abstract class ValidationErrorInfoBase: IValidationErrorInfo
    {

        #region Fields

        private readonly object balanceLock = new();
        private Dictionary<string, string> errColumns = [];

        #endregion Fields

        #region Properties

        /// <summary>
        /// Mustra el error general de la fila
        /// </summary>
        [Display(AutoGenerateField = false)]
        public string Error { get; private set; } = string.Empty;

        /// <summary>
        /// Mantiene el valor de validaión
        /// </summary>
        [Display(AutoGenerateField = false)]
        public bool HasValidationErrors { get; set; }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Obtiene el mensaje de error de la columna indicada
        /// </summary>
        /// <returns></returns>
        [Display(AutoGenerateField = false)]
        public string this[string columnName] => GetErrorMessage(columnName);

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Agrega un mensaje de error a la coumna
        /// </summary>
        /// <param name="ErrMessage"></param>
        /// <param name="ColumnName"></param>
        [Display(AutoGenerateField = false)]
        public virtual void AddError(string ErrMessage, string ColumnName)
        {
            if (!errColumns.ContainsKey(ColumnName))
            {
                errColumns[ColumnName] = ErrMessage;
            }
        }

        /// <summary>
        /// Borra todos los mensajes del objeto
        /// </summary>
        [Display(AutoGenerateField = false)]
        public virtual void ClearErrorInfo()
        {
            ((IValidationErrorInfo)this).HasValidationErrors = false;
            Error = string.Empty;
            if (errColumns != null && errColumns.Count > 0) errColumns.Clear();
            errColumns = null;
        }

       
        /// <summary>
        /// Establece el mensaje de error del objeto
        /// </summary>
        [Display(AutoGenerateField = false)]
        public virtual void SetError(string errMessage)
        {
            Error = errMessage;
        }

        /// <summary>
        /// Obtiene el mensaje de error establecido por el atributo.
        /// </summary>
        [Display(AutoGenerateField = false)]
        internal virtual string GetErrorMessage(string columnName)
        {
            string ret;
            lock (balanceLock)
            {
                ret = GetErrorMessageCore(columnName);
            }
            return ret;
        }
        private string GetErrorMessageCore(string columnName)
        {
            if (errColumns == null) errColumns = new Dictionary<string, string>();
            IValidationErrorInfo errInfo = this;
            string ret = string.Empty;
            if (!errColumns.ContainsKey(columnName))
            {
                string ErrMsg = this.GetErrorText(columnName);
                if (!string.IsNullOrEmpty(ErrMsg)) errColumns.Add(columnName, ErrMsg);
            }
            else
            {
                ret = errColumns?[columnName]??string.Empty;
            }
            return ret;
        }
        /// <summary>
        /// Explora las propiedades del objeto en busca de atributos de validacion
        /// </summary>
        /// <param name="Deep"></param>
        /// <param name="debugProperty"></param>
        /// <returns></returns>
        protected internal virtual bool HasErrors(int Deep = 2, bool debugProperty = false)
        {
            IValidationErrorInfo errInfo = this;
            errInfo.ClearErrorInfo();

            errInfo.HasValidationErrors = this.HasErrors(Deep, debugProperty)
                                          || !string.IsNullOrEmpty(Error)
                                          || (errColumns != null && errColumns.Count > 0);
            return errInfo.HasValidationErrors;
        }
        #endregion Methods
    }
}
