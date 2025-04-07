using System.ComponentModel;
using System.Reflection;

namespace KUtilitiesCore.Tracking
{
    /// <summary>
    /// Clase base que implementa IEditableObject para gestionar el seguimiento y reversión de cambios
    /// </summary>
    public abstract class ChangeTrackingBase : INotifyPropertyChanged, IEditableObject
    {
        #region Fields

        [NonSerialized]
        private Dictionary<string, TrackedProperty> memento;

        #endregion Fields

        #region Events

        // Memento: almacena el estado en BeginEdit
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Indica si se han modificado los valores de las propiedades despúes de Invocar BeginEdit
        /// </summary>
        public bool IsChanged { get => memento != null && memento.Any(x => x.Value.IsChanged(this)); }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Se captura el estado actual de las propiedades (se puede personalizar según necesidades)
        /// </summary>
        public void BeginEdit()
        {
            if (memento == null)
                memento = new Dictionary<string, TrackedProperty>();
            Track(GetTrackableProperties());
        }

        /// <summary>
        /// Restaura los valores de las propiedades modificadas y fiinaliza la edición
        /// </summary>
        public void CancelEdit()
        {
            if (memento != null)
            {
                foreach (KeyValuePair<string, TrackedProperty> p in memento)
                {
                    p.Value.Property.SetValue(this, p.Value.Value);
                    OnPropertyChanged(p.Key);
                }
            }
            EndEdit();
        }

        /// <summary>
        /// Establece los valores actuales como finales
        /// </summary>
        public void EndEdit()
        {
            memento?.Clear();
            memento = null;
        }

        // Método para notificar cambios en las propiedades
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Obtiene una lista de propiedades que pueden ser rastreadas.
        /// </summary>
        /// <returns></returns>

        private IEnumerable<PropertyInfo> GetTrackableProperties()
        {
            return GetType().GetProperties()
                .Where(p =>
                    !p.DeclaringType.Equals(typeof(ChangeTrackingBase))
                    && p.CanRead
                    && p.CanWrite
                    && !p.GetCustomAttributes<ChangeTrackerIgnoreAttribute>(false).Any()
                );
        }

        /// <summary>
        /// Comienza a rastrear las propiedades especificadas.
        /// </summary>
        /// <param name="properties"></param>
        private void Track(IEnumerable<PropertyInfo> properties)
        {
            if (properties != null && properties.Any())
            {
                foreach (PropertyInfo prop in properties)
                {
                    TrackProperty(prop);
                }
            }
        }

        private void TrackProperty(PropertyInfo prop)
        {
            memento[prop.Name] = new TrackedProperty(prop, this);
        }

        #endregion Methods

        #region Classes

        private class TrackedProperty
        {
            #region Properties

            public TrackedProperty(PropertyInfo property, ChangeTrackingBase srcObject)
            {
                Property = property;
                Value = property.GetValue(srcObject, null);
            }

            public PropertyInfo Property { get; set; }
            public object Value { get; set; }

            #endregion Properties

            #region Methods

            internal bool IsChanged(ChangeTrackingBase srcObject)
            {
                return !Equals(Value, Property.GetValue(srcObject, null));
            }

            #endregion Methods
        }

        #endregion Classes
    }
}