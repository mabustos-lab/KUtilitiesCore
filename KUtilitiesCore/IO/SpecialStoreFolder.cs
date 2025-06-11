using System;
using System.Linq;

namespace KUtilitiesCore.IO
{
    /// <summary>
    /// Establece las carpetas locales especiales de almacenamiento
    /// </summary>
    public enum SpecialStoreFolder
    {
        /// <summary>
        /// Directorio que sirve de repositorio común de datos específicos de la aplicación
        /// para el usuario móvil actual.
        /// </summary>
        ApplicationData = 26,
        /// <summary>
        /// Directorio que sirve de repositorio común de datos específicos de la aplicación
        /// que todos los usuarios utilizan.
        /// </summary>
        CommonApplicationData = 35,
        /// <summary>
        /// Directorio que sirve de repositorio común para datos específicos de la aplicación
        /// que el usuario no móvil actual utiliza.
        /// </summary>
        LocalApplicationData = 28,
        /// <summary>
        /// Almacenamiento aislado con ámbito en el Equipo
        /// </summary>
        MachineIsolatedStorage = 1,
        /// <summary>
        /// Almacenamiento aislado con ámbito en el usuario
        /// </summary>
        UserIsolatedStorage = 2,
        /// <summary>
        /// Obtiene la carpeta publica que todos los usuarios tienen acceso %PUBLIC%
        /// </summary>
        AllUserPublic = 3,
        /// <summary>
        /// Representa la carpeta de almacenamiento temporal del sistema.
        /// </summary>
        TemporalFolder = 4
    }
}
