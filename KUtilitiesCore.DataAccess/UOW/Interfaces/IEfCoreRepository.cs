using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Interfaz específica para repositorios que soportan operaciones masivas de EF Core 7+.
    /// Hereda de IRepository para incluir todas las operaciones estándar.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad.</typeparam>
    public interface IEfCoreRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
#if NETCOREAPP // Estos métodos son específicos de EF Core 7+
        /// <summary>
        /// Ejecuta una operación de actualización masiva en la base de datos basada en la especificación.
        /// ADVERTENCIA: Este método es específico para implementaciones que soportan EF Core 7.0+.
        /// </summary>
        /// <param name="specification">La especificación que define qué entidades actualizar.</param>
        /// <param name="updates">Una colección de descriptores de actualización de propiedad.</param>
        /// <returns>El número de filas afectadas.</returns>
        Task<int> ExecuteUpdateAsync(ISpecification<TEntity> specification, IEnumerable<PropertyUpdateDescriptor<TEntity>> updates);

        /// <summary>
        /// Ejecuta una operación de eliminación masiva en la base de datos basada en la especificación.
        /// ADVERTENCIA: Este método es específico para implementaciones que soportan EF Core 7.0+.
        /// </summary>
        /// <param name="specification">La especificación que define qué entidades eliminar.</param>
        /// <returns>El número de filas afectadas.</returns>
        Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification);

#endif
    }
}
