using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Dal.Helpers
{
    /// <summary>
    /// Opciones de configuración para el mapeo
    /// </summary>
    internal class TranslateOptions
    {
        /// <summary>
        /// Si es true, lanza excepción cuando no se pueden mapear todas las propiedades requeridas
        /// </summary>
        public bool StrictMapping { get; set; } = false;

        /// <summary>
        /// Lista de propiedades que deben mapearse obligatoriamente (solo aplica en StrictMapping = true)
        /// </summary>
        public string[] RequiredProperties { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Si es true, ignora propiedades que no existen en el resultado del reader
        /// </summary>
        public bool IgnoreMissingColumns { get; set; } = false;

        /// <summary>
        /// Prefijos a remover de los nombres de columna antes del mapeo
        /// </summary>
        public string[] ColumnPrefixesToRemove { get; set; } = Array.Empty<string>();
    }
}
