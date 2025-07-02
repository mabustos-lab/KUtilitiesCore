using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data
{
    public interface IValidationErrorInfo:IDataErrorInfo
    {
        /// <summary>
        /// Indica si hay algún mensaje de error en el objeto
        /// </summary>
        [NotMapped]
        [Display(AutoGenerateField = false)]
        bool HasValidationErrors { get; set; }
        /// <summary>
        /// Agrega una mensaje de error en una propiedad
        /// </summary>
        /// <param name="ErrMessage"></param>
        /// <param name="ColumnName"></param>
        void AddError(string ErrMessage, string ColumnName);
        /// <summary>
        /// Borra todos los mensajes de error de cada propiedad
        /// </summary>
        void ClearErrorInfo();
        /// <summary>
        /// Establece un mensaje de error del objeto
        /// </summary>
        /// <param name="errMessage"></param>
        void SetError(string errMessage);
    }
}
