using System;
using System.Collections.Generic;
using System.Text;

namespace KUtilitiesCore.DataAccess.UOW.Interfaces
{
    /// <summary>
    /// Define un proveedor para obtener instancias de repositorio genéricas para un tipo de entidad especificado.
    /// </summary>
    /// <remarks>Las implementaciones de esta interfaz son responsables de proporcionar repositorios que admitan el patrón de especificación.
    /// Esta abstracción permite a los consumidores solicitar repositorios sin necesidad de conocer la implementación de acceso a datos subyacente.
    /// </remarks>
    public interface IRepositoryProvider
    {
        /// <summary>
        /// Obtiene una instancia del repositorio genérico para la entidad especificada.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <returns>Instancia de IRepository con soporte para Specifications.</returns>
        IRepository<T> Repository<T>() where T : class;
        /// <summary>
        /// Obtiene un repositorio de lectura registrado.
        /// </summary>
        /// <typeparam name="TRepo">El tipo de repositorio a obtener.</typeparam>
        /// <returns>La instancia del repositorio.</returns>
        /// <exception cref="InvalidOperationException">Si el repositorio personalizado no ha sido registrado previamente.</exception>
        TRepo RawRepository<TRepo>() where TRepo : IRawRepository;
    }
}
