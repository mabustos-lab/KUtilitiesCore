using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace KUtilitiesCore.Tracking
{
    /// <summary>
    /// Clase base que implementa IEditableObject para gestionar el seguimiento y reversión de cambios.
    /// </summary>
    public abstract class ChangeTrackingBase : INotifyPropertyChanged, IEditableObject
    {
        #region Fields

        [NonSerialized]
        private readonly Stack<Dictionary<string, TrackedProperty>> _snapshotStack = new();

        #endregion

        #region Events

        /// <summary>
        /// Evento que se produce cuando el valor de una propiedad cambia.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Indica si se han modificado los valores de las propiedades después de invocar BeginEdit.
        /// </summary>
        public bool IsChanged => CurrentSnapshot?.Any(x => x.Value.IsChanged(this)) ?? false;

        private Dictionary<string, TrackedProperty>? CurrentSnapshot =>
            _snapshotStack.Count > 0 ? _snapshotStack.Peek() : null;

        #endregion

        #region Methods

        /// <summary>
        /// Captura el estado actual de las propiedades para realizar el seguimiento de cambios.
        /// </summary>
        public void BeginEdit()
        {
            var snapshot = CreateCleanSnapshot();
            _snapshotStack.Push(snapshot);
        }

        /// <summary>
        /// Restaura los valores de las propiedades modificadas y finaliza la edición.
        /// </summary>
        public void CancelEdit()
        {
            if (_snapshotStack.Count == 0) return;

            var snapshot = _snapshotStack.Pop();
            RestoreSnapshot(snapshot);
        }

        /// <summary>
        /// Establece los valores actuales como finales y limpia el historial de cambios.
        /// </summary>
        public void EndEdit()
        {
            if (_snapshotStack.Count > 0)
                _snapshotStack.Pop();
        }

        /// <summary>
        /// Notifica un cambio en el valor de una propiedad usando una expresión lambda.
        /// </summary>
        /// <typeparam name="T">Tipo de la propiedad</typeparam>
        /// <param name="propertyExpression">Expresión que identifica la propiedad</param>
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null) return;

            string propertyName = memberExpression.Member.Name;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Notifica un cambio en el valor de una propiedad.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Helper Methods

        private Dictionary<string, TrackedProperty> CreateCleanSnapshot()
        {
            var snapshot = new Dictionary<string, TrackedProperty>();
            var currentTrackableProperties = GetTrackableProperties().ToList();

            // Limpieza de propiedades obsoletas en snapshots previos
            if (_snapshotStack.Count > 0)
            {
                var previousSnapshot = _snapshotStack.Peek();
                foreach (var key in previousSnapshot.Keys.ToList())
                {
                    if (!currentTrackableProperties.Any(p => p.Name == key))
                        previousSnapshot.Remove(key);
                }
            }

            foreach (var prop in currentTrackableProperties)
            {
                snapshot[prop.Name] = new TrackedProperty(prop, this);
            }
            return snapshot;
        }

        private void RestoreSnapshot(Dictionary<string, TrackedProperty> snapshot)
        {
            foreach (var entry in snapshot)
            {
                entry.Value.Property.SetValue(this, entry.Value.Value);
                OnPropertyChanged(entry.Key);
            }
        }

        private IEnumerable<PropertyInfo> GetTrackableProperties()
        {
            return GetType()
                .GetProperties()
                .Where(IsTrackableProperty);
        }

        private bool IsTrackableProperty(PropertyInfo property)
        {
            return property.DeclaringType != typeof(ChangeTrackingBase) &&
                   property.CanRead &&
                   property.CanWrite &&
                   !property.GetCustomAttributes<ChangeTrackerIgnoreAttribute>(false).Any();
        }

        #endregion

        #region Inner Classes

        private sealed class TrackedProperty
        {
            public PropertyInfo Property { get; }
            public object? Value { get; }

            public TrackedProperty(PropertyInfo property, ChangeTrackingBase srcObject)
            {
                Property = property;
                Value = property.GetValue(srcObject, null);
            }

            internal bool IsChanged(ChangeTrackingBase srcObject)
            {
                return !Equals(Value, Property.GetValue(srcObject, null));
            }
        }

        #endregion
    }
}