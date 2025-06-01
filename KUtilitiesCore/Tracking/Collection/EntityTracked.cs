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

        public EntityTracked(TEntity entity, TrackedStatus status = TrackedStatus.UnModified)
        {
            Entity = entity;
            Status = status;
        }

        #endregion Constructors

        #region Properties

        public TEntity Entity { get; set; }
        public TrackedStatus Status { get; set; }

        #endregion Properties
    }
}