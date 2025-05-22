using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KUtilitiesCore.Tracking.Collection
{
    public interface ITrackedCollection<TEntity> : IEnumerable<TEntity>
        where TEntity : class, INotifyPropertyChanged
    {

        /// <summary>
        /// Indica si alguno de los elementos rastrados fue modificado.
        /// </summary>
        bool HasChanges { get; }
        /// <summary>
        /// Indica el inicio de un bloque donde todos los elementos insertados son marcados como no modificados.
        /// </summary>
        void BeginInsertUnmodifiedEntities();
        /// <summary>
        /// Indica el final de un bloque donde todos los elementos insertados son marcados como no modificados.
        /// </summary>
        void EndInsertUnmodifiedEntities();
        /// <summary>
        /// Obtiene un elemento rastreado específico de la colección.
        /// </summary>
        /// <param name="entity">El elemento a buscar.</param>
        /// <returns>El elemento rastreado si se encuentra; de lo contrario, <c>null</c>.</returns>
        EntityTracked<TEntity>? GetTrackedItem(TEntity entity);
        /// <summary>
        /// Obtiene una lista de elementos rastreados en la colección.
        /// </summary>
        /// <returns>Una lista de solo lectura de elementos rastreados.</returns>
        IReadOnlyList<EntityTracked<TEntity>> GetTrackedItems();

    }
}