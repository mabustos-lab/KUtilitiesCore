using System;

namespace KUtilitiesCore.Logger.Options
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
        /// <summary>
        /// Establece el Máximo en dias de vida de un archivo Log
        /// </summary>
        public int RetentionDays { get; set; } = 30;
        /// <summary>
        /// Establece la ruta raiz del archivo Log
        /// </summary>
        public string LogDirectory { get; internal set; } = string.Empty;
        /// <summary>
        /// Indica si el formato de almacenamiento sea una estructura JSON o solo Texto
        /// </summary>
        public bool UseJSonFormat { get; internal set; } = false;
        public string ApplicationName { get; internal set; } = "Application";
    }
}
