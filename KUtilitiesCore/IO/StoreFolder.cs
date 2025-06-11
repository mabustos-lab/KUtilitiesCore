using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.IO
{
    public static class StoreFolder
    {
        /// <summary>
        /// Obtiene la ruta especial de almacenamiento predefinido según el tipo de carpeta especificado.
        /// </summary>
        /// <param name="folder">El tipo de carpeta especial de almacenamiento.</param>
        /// <returns>La ruta de la carpeta especial de almacenamiento como una cadena de texto.</returns>
        public static string GetSpecialStoreFolder(SpecialStoreFolder folder)
        {
            return folder switch
            {
                SpecialStoreFolder.ApplicationData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Empty),
                SpecialStoreFolder.CommonApplicationData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), string.Empty),
                SpecialStoreFolder.LocalApplicationData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), string.Empty),
                SpecialStoreFolder.AllUserPublic => Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments))?.FullName??"", string.Empty),
                SpecialStoreFolder.MachineIsolatedStorage => GetIsolatedStorageRoot(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly),
                SpecialStoreFolder.UserIsolatedStorage => GetIsolatedStorageRoot(IsolatedStorageScope.User | IsolatedStorageScope.Assembly),
                SpecialStoreFolder.TemporalFolder => Path.GetTempPath(),
                _ => string.Empty,
            };
        }
        /// <summary>
        /// Obtiene la ruta raíz del almacenamiento aislado usando reflexión para diferentes versiones de .NET
        /// </summary>
        /// <param name="scope">Ámbito del almacenamiento aislado</param>
        /// <returns>Ruta del directorio raíz o string.Empty si no se encuentra</returns>
        private static string GetIsolatedStorageRoot(IsolatedStorageScope scope)
        {
            using var isoStore = IsolatedStorageFile.GetStore(scope, null, null);

            // Intento para .NET Framework: campo interno 'm_RootDir'
            var rootDir = GetMemberValue(isoStore, "m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!string.IsNullOrEmpty(rootDir)) return rootDir!;

            // Intento para .NET 8+: propiedad 'RootDirectory' (puede ser pública o interna)
            rootDir = GetMemberValue(isoStore, "RootDirectory", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            return rootDir ?? string.Empty;
        }

        /// <summary>
        /// Intenta obtener el valor de un miembro (campo o propiedad) usando reflexión
        /// </summary>
        /// <param name="target">Objeto donde buscar el miembro</param>
        /// <param name="memberName">Nombre del miembro a buscar</param>
        /// <param name="flags">Bandera de búsqueda para reflexión</param>
        /// <returns>Valor del miembro como string o null si no existe</returns>
        private static string? GetMemberValue(IsolatedStorageFile target, string memberName, BindingFlags flags)
        {
            var type = target.GetType();

            // Primero intentar como campo
            var field = type.GetField(memberName, flags);
            if (field != null)
                return field.GetValue(target) as string;

            // Si no se encuentra, intentar como propiedad
            var property = type.GetProperty(memberName, flags);
            if (property != null)
                return property.GetValue(target) as string;

            return null;
        }
    }
}
