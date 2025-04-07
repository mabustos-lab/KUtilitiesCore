using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KUtilitiesCore.Tracking.Collection
{
    /// <summary>
    /// Representa una colección que rastrea los cambios en sus elementos.
    /// </summary>
    /// <typeparam name="TEntity">
    /// El tipo de los elementos en la colección, que debe implementar <see cref="INotifyPropertyChanged"/>.
    /// </typeparam>
    public class TrackedCollection<TEntity> : ObservableCollection<TEntity> where TEntity : class, INotifyPropertyChanged
    {
        #region Fields

        private readonly List<EntityTracked<TEntity>> _entityItems;
        private bool BeginInsertUnmodifiedItems;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TrackedCollection{TEntity}"/>.
        /// </summary>
        public TrackedCollection()
        {
            BeginInsertUnmodifiedItems = false;
            _entityItems = new List<EntityTracked<TEntity>>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Indica el inicio de un bloque donde todos los elementos insertados son marcados como no modificados.
        /// </summary>
        public void BeginInsertUnmodifiedEntities()
        {
            BeginInsertUnmodifiedItems = true;
        }

        /// <summary>
        /// Indica el final de un bloque donde todos los elementos insertados son marcados como no modificados.
        /// </summary>
        public void EndInsertUnmodifiedEntities()
        {
            BeginInsertUnmodifiedItems = false;
        }

        /// <summary>
        /// Obtiene un elemento rastreado específico de la colección.
        /// </summary>
        /// <param name="entity">El elemento a buscar.</param>
        /// <returns>El elemento rastreado si se encuentra; de lo contrario, <c>null</c>.</returns>
        public EntityTracked<TEntity> GetTrackedItem(TEntity entity)
            => _entityItems.FirstOrDefault(x => x.Entity.Equals(entity));

        /// <summary>
        /// Obtiene una lista de elementos rastreados en la colección.
        /// </summary>
        /// <returns>Una lista de solo lectura de elementos rastreados.</returns>
        public IReadOnlyList<EntityTracked<TEntity>> GetTrackedItems()
        {
            return _entityItems.AsReadOnly();
        }

        /// <summary>
        /// Elimina todos los elementos de la colección y desuscribe los eventos de cambio de propiedad.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var wrapper in _entityItems)
            {
                if (wrapper != null && wrapper.Entity != null)
                {
                    wrapper.Entity.PropertyChanged -= OnItemValueChanged;
                }
            }
            _entityItems.Clear();
            base.ClearItems();
        }

        /// <summary>
        /// Inserta un elemento en la colección en el índice especificado.
        /// </summary>
        /// <param name="index">El índice en el que se debe insertar el elemento.</param>
        /// <param name="item">El elemento a insertar.</param>
        protected override void InsertItem(int index, TEntity item)
        {
            TrackedStatus status = BeginInsertUnmodifiedItems ? TrackedStatus.None : TrackedStatus.Added;
            EntityTracked<TEntity> wrapper = new EntityTracked<TEntity>(item, status);

            item.PropertyChanged += OnItemValueChanged;

            _entityItems.Add(wrapper);

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Elimina el elemento en el índice especificado.
        /// </summary>
        /// <param name="index">El índice del elemento a eliminar.</param>
        protected override void RemoveItem(int index)
        {
            var wrapper = _entityItems.FirstOrDefault(x => x.Entity.Equals(this.Items[index]));
            if (wrapper != null && wrapper.Entity != null)
            {
                wrapper.Entity.PropertyChanged -= OnItemValueChanged;
            }
            if (wrapper != null)
            {
                if (wrapper.Status == TrackedStatus.Added)
                    _entityItems.Remove(wrapper);
                else
                    wrapper.Status = TrackedStatus.Removed;
            }
            base.RemoveItem(index);
        }

        /// <summary>
        /// Reemplaza el elemento en el índice especificado.
        /// </summary>
        /// <param name="index">El índice del elemento a reemplazar.</param>
        /// <param name="item">El nuevo elemento.</param>
        protected override void SetItem(int index, TEntity item)
        {
            // Eliminar la suscripción del elemento reemplazado
            var oldWrapper = _entityItems.FirstOrDefault(x => x.Entity.Equals(this.Items[index]));

            if (oldWrapper != null && oldWrapper.Entity != null)
            {
                oldWrapper.Entity.PropertyChanged -= OnItemValueChanged;
                if (oldWrapper.Status == TrackedStatus.Added)
                    _entityItems.Remove(oldWrapper);
                else
                    oldWrapper.Status = TrackedStatus.Removed;
            }

            base.SetItem(index, item);

            // Crear el nuevo wrapper
            var newWrapper = new EntityTracked<TEntity>(item)
            {
                Status = TrackedStatus.Added // se marca como agregado (o bien, podrías elegir otro valor según la lógica)
            };

            item.PropertyChanged += OnItemValueChanged;

            _entityItems.Add(newWrapper);
        }

        /// <summary>
        /// Maneja el evento de cambio de propiedad de un elemento.
        /// </summary>
        /// <param name="wrapper">El contenedor del elemento rastreado.</param>
        /// <param name="e">Los datos del evento.</param>
        private void OnItemValueChanged(object sender, PropertyChangedEventArgs e)
        {
            EntityTracked<TEntity> wrapper = (EntityTracked<TEntity>)sender;
            if (wrapper.Status != TrackedStatus.Added || wrapper.Status != TrackedStatus.Removed)
            {
                wrapper.Status = TrackedStatus.Modified;
            }
        }

        #endregion Methods
    }
}