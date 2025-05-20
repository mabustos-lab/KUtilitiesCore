using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz para la Unidad de Trabajo (Unit of Work).
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Obtiene una instancia de un repositorio genérico para un tipo de entidad específico.
        /// </summary>
        IRepository<TEntity, TPrimaryKey> GetRepository<TEntity, TPrimaryKey>() where TEntity : class;

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
