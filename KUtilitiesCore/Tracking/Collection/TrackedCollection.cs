﻿using System.Collections.Generic;
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
    public class TrackedCollection<TEntity> : ObservableCollection<TEntity>,ITrackedCollection<TEntity> where TEntity : class, INotifyPropertyChanged
    {

        private readonly Dictionary<TEntity, EntityTracked<TEntity>> _entityItems;
        private bool BeginInsertUnmodifiedItems;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TrackedCollection{TEntity}"/>.
        /// </summary>
        public TrackedCollection()
        {
            BeginInsertUnmodifiedItems = false;
            _entityItems = [];
        }

        /// <summary>
        /// Indica si alguno de los elementos rastrados fue modificado.
        /// </summary>
        public bool HasChanges
            => _entityItems.Count > 0 && _entityItems.Values.Any(x => x.Status != TrackedStatus.UnModified);

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
        public EntityTracked<TEntity>? GetTrackedItem(TEntity entity)
        {
            _entityItems.TryGetValue(entity, out EntityTracked<TEntity>? wrapper);
            return wrapper;
        }

        /// <summary>
        /// Obtiene una lista de elementos rastreados en la colección.
        /// </summary>
        /// <returns>Una lista de solo lectura de elementos rastreados.</returns>
        public IReadOnlyList<EntityTracked<TEntity>> GetTrackedItems()
        {
            return _entityItems.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Elimina todos los elementos de la colección y desuscribe los eventos de cambio de propiedad.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var wrapper in _entityItems.Values)
            {
                if (wrapper?.Entity != null)
                {
                    wrapper.Entity.PropertyChanged -= new PropertyChangedEventHandler(OnItemValueChanged);
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
            TrackedStatus status = BeginInsertUnmodifiedItems
            ? TrackedStatus.UnModified
            : TrackedStatus.Added;

            if (!_entityItems.TryGetValue(item, out EntityTracked<TEntity>? wrapper))
            {
                wrapper = new EntityTracked<TEntity>(item, status);
                _entityItems[item] = wrapper;
                item.PropertyChanged += OnItemValueChanged;
            }
            else
            {
                // Si el item ya existe, decide cómo manejarlo
                wrapper.Status = status;
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Elimina el elemento en el índice especificado.
        /// </summary>
        /// <param name="index">El índice del elemento a eliminar.</param>
        protected override void RemoveItem(int index)
        {
            TEntity? itemToRemove = Items[index];
            if (itemToRemove != null && _entityItems.TryGetValue(itemToRemove, out EntityTracked<TEntity>? wrapper) && wrapper != null)
            {
                if (wrapper.Entity != null)
                {
                    wrapper.Entity.PropertyChanged -= OnItemValueChanged!;
                }

                if (wrapper.Status == TrackedStatus.Added)
                {
                    _entityItems.Remove(itemToRemove);
                }
                else
                {
                    wrapper.Status = TrackedStatus.Removed;
                }
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
            // Eliminar el item antiguo
            TEntity oldItem = Items[index];
            if (_entityItems.TryGetValue(oldItem, out EntityTracked<TEntity>? oldWrapper) && oldWrapper is not null)
            {
                oldWrapper.Entity.PropertyChanged -= OnItemValueChanged!;
                _entityItems.Remove(oldItem);
            }

            // Agregar el nuevo item
            TrackedStatus status = BeginInsertUnmodifiedItems
                ? TrackedStatus.UnModified
                : TrackedStatus.Added;

            EntityTracked<TEntity> newWrapper = new(item, status);

            item.PropertyChanged += OnItemValueChanged!;
            _entityItems[item] = newWrapper;

            base.SetItem(index, item);
        }

        /// <summary>
        /// Maneja el evento de cambio de propiedad de un elemento.
        /// </summary>
        /// <param name="sender">El objeto que generó el evento.</param>
        /// <param name="e">Los datos del evento.</param>
        private void OnItemValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TEntity entity)
                return;

            if (!_entityItems.TryGetValue(entity, out EntityTracked<TEntity>? wrapper) || wrapper is null)
                return;

            if (wrapper.Status == TrackedStatus.Added || wrapper.Status == TrackedStatus.Removed)
                return;

            wrapper.Status = TrackedStatus.Modified;
        }

    }
}