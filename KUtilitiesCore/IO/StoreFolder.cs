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
                SpecialStoreFolder.AllUserPublic => Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)).FullName, string.Empty),
                SpecialStoreFolder.MachineIsolatedStorage => GetIsolatedStorageRoot(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly),
                SpecialStoreFolder.UserIsolatedStorage => GetIsolatedStorageRoot(IsolatedStorageScope.User | IsolatedStorageScope.Assembly),
                _ => string.Empty,
            };
        }

        private static string GetIsolatedStorageRoot(IsolatedStorageScope scope)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(scope, null, null))
            {
                return isoStore.GetType().GetField("m_RootDir", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(isoStore).ToString();
            }
        }
    }
}
