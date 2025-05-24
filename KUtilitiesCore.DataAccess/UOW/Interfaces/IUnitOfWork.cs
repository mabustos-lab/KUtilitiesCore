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
        ///// <summary>
        ///// Obtiene el repositorio de productos. La implementación decidirá si devuelve
        ///// un IProductRepository o un IEfCoreProductRepository si se necesita funcionalidad masiva.
        ///// </summary>
        //IProductRepository Products { get; } // Se mantiene como IProductRepository por ahora

        // Considerar cómo exponer repositorios que implementan IEfCoreRepository.
        // Opción 1: Un método GetRepository diferente o con un type constraint más específico.
        // Opción 2: Que los repositorios específicos devueltos por propiedades (como Products)
        // implementen IEfCoreRepository si es aplicable, y el consumidor haga un type cast seguro.

        /// <summary>
        /// Obtiene una instancia de un repositorio genérico para un tipo de entidad específico.
        /// </summary>
        IRepository<TEntity, TPrimaryKey> GetRepository<TEntity, TPrimaryKey>() where TEntity : class;

        /// <summary>
        /// Obtiene una instancia de un repositorio con capacidades de EF Core (operaciones masivas)
        /// si la implementación lo soporta. Devuelve null o lanza excepción si no.
        /// </summary>
        IEfCoreRepository<TEntity, TPrimaryKey> GetEfCoreRepository<TEntity, TPrimaryKey>() where TEntity : class;


        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
