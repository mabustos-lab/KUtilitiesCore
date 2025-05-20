using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Opciones adicionales para un logger que escribe en archivos, incluyendo tamaño máximo y retención.
    /// </summary>
    public class FileLoggerOptions : LoggerOptions
    {
        /// <summary>
        /// Obtiene o establece el tamaño máximo del archivo de log en bytes.
        /// </summary>
        public int MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

        /// <summary>
        /// Obtiene o establece la cantidad máxima de archivos de log retenidos.
        /// </summary>
        public int MaxRetainedFiles { get; set; } = 5;
    }
}
