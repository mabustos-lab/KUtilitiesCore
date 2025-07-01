using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

#if WINDOWS
namespace KUtilitiesCore.Utils
{
    /// <summary>
    /// Contiene funciones especialzadas para el manejo de la memoria
    /// </summary>
    public static class MemoryHelper
    {

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        /// <summary>
        /// Intenta reducir el conjunto de trabajo (working set) del proceso actual,
        /// liberando memoria física no utilizada.
        /// Solo tiene efecto en sistemas Windows.
        /// </summary>
        public static void CompactCurrentProcessWorkingSet()
        {
            GC.Collect();                  // 1. Recolecta objetos no referenciados
            GC.WaitForPendingFinalizers(); // 2. Espera finalizadores (opcional)
            GC.Collect();                  // 3. Segundo collect para los objetos que quedaron listos

            IntPtr handle = GetCurrentProcess();
            // -1 indica al sistema que ajuste automáticamente los tamaños mínimos y máximos
            SetProcessWorkingSetSize(handle, -1, -1);
        }

    }
}
#endif