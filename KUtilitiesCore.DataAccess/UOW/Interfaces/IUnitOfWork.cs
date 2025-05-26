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
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        /// <summary>
        /// Obtiene una instancia de un repositorio con capacidades de EF Core (operaciones masivas)
        /// si la implementación lo soporta. Devuelve null o lanza excepción si no.
        /// </summary>
        IEfCoreRepository<TEntity> GetEfCoreRepository<TEntity>() where TEntity : class;


        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
