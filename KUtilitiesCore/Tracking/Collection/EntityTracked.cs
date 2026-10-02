using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Tracking.Collection
{
    /// <summary>
    /// Clase para encapsular un elemento con su estado
    /// </summary>
    public class EntityTracked<TEntity> where TEntity : class, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Inicializa el contenedor asociando la entidad con su estado inicial de
        /// seguimiento.
        /// </summary>
        /// <param name="entity">Entidad a rastrear.</param>
        /// <param name="status">Estado inicial de seguimiento; por defecto no modificada.</param>
        public EntityTracked(TEntity entity, TrackedStatus status = TrackedStatus.UnModified)
        {
            Entity = entity;
            Status = status;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Entidad rastreada.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <summary>
        /// Estado de seguimiento actual de la entidad dentro de la colección.
        /// </summary>
        public TrackedStatus Status { get; set; }

        #endregion Properties
    }
}