using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter.Interfaces
{
    /// <summary>
    /// Interfaz abstracta para operaciones de archivos en disco
    /// </summary>
    public interface IDiskFileReader
    {
        /// <summary>
        /// Verifica si un archivo existe
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <returns>True si el archivo existe</returns>
        bool FileExists(string path);

        /// <summary>
        /// Abre un archivo para lectura
        /// </summary>
        /// <param name="path">Ruta del archivo</param>
        /// <returns>Stream de lectura</returns>
        Stream OpenRead(string path);
    }
}
