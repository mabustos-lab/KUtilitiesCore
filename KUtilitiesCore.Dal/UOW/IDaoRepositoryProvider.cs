using KUtilitiesCore.DataAccess.UOW.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.UOW
{
    /// <summary>
    /// Proporciona acceso a repositorios registrados, lo que permite recuperar una instancia de repositorio para un
    /// tipo especificado. Si se solicita la interfaz base IRawRepository y no está registrada, se crea una instancia predeterminada.
    /// </summary>
    public interface IDaoRepositoryProvider:IRepositoryProvider
    {
        /// <summary>
        /// Obtiene un repositorio de lectura registrado.
        /// </summary>
        /// <typeparam name="TRepo">El tipo de repositorio a obtener.</typeparam>
        /// <returns>La instancia del repositorio.</returns>
        /// <exception cref="InvalidOperationException">Si el repositorio personalizado no ha sido registrado previamente.</exception>
        TRepo RawRepository<TRepo>() where TRepo : IRawRepository;
    }
}
