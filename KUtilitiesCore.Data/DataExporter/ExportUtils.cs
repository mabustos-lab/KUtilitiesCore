using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataExporter
{
    /// <summary>
    /// Métodos auxiliares internos para los procesos de exportación.
    /// </summary>
    internal static class ExportUtils
    {
        /// <summary>
        /// Abre el archivo con la aplicación predeterminada del sistema.
        /// </summary>
        public static void OpenFile(string filePath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.UseShellExecute = true; // Necesario para .NET Core / 5+
                process.Start();
            }
            catch (Exception ex)
            {
                Debug.Write($"No se pudo abrir el archivo automáticamente: {ex.Message}");
                // No lanzamos error aquí, ya que el archivo sí se generó correctamente.
            }
        }
        public static string GetValidSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Hoja1";

            string validName = name;
            if (validName.Length > 31) validName = validName.Substring(0, 31);

            char[] invalidChars = { ':', '\\', '/', '?', '*', '[', ']' };
            foreach (char c in invalidChars) validName = validName.Replace(c, '_');

            return validName.Trim('\'');
        }
    }
}
