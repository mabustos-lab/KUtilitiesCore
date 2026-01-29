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
    }
}
