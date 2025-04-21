using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Helpers
{
    /// <summary>
    /// Emcapsula el contexto de cambio de valores para validar si se requiere el cambio antes de aplicarlos
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class OnPropertyChangingArg<TContext, TProperty>
        : EventArgs
    {
        #region Constructors

        public OnPropertyChangingArg(
                    string propertyName, TProperty OldValue, TProperty NewValue)
        {
            this.PropertyName = propertyName;
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Indica si se calncela la actualizacion del Antiguo valor por el Nuevo valor en la propiedad
        /// </summary>
        public bool Cancel { get; set; } = false;
        public TProperty NewValue { get; private set; }
        public TProperty OldValue { set; private get; }
        public string PropertyName { get; private set; }

        #endregion Properties
    }
}