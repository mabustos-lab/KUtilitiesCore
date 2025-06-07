using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using KUtilitiesCore.Logger.Info;

namespace KUtilitiesCore.Logger.Info
{
    /// <summary>
    /// Clase especializada en extraer y formatear información estructurada de excepciones.
    /// Representa la información detallada de una excepción para su análisis o presentación.
    /// </summary>
    public class ExceptionInfo
    {

        private const int IndentWidth = 4; // Define la cantidad de espacios por nivel de indentación.
        private readonly int _currentExceptionDepth; // Profundidad de esta instancia de excepción en una jerarquía.

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ExceptionInfo"/> con valores predeterminados.
        /// </summary>
        public ExceptionInfo()
        {
            AggregateExceptions = [];
            AdditionalInfo = [];
            _currentExceptionDepth = 1; // Profundidad base para una excepción no anidada directamente.

            // Inicializa propiedades no nulables con valores predeterminados para asegurar consistencia.
            ExceptionId = string.Empty;
            BaseExceptionMessage = string.Empty;
            ExceptionMessage = string.Empty;
            Source = string.Empty;
            TargetSite = string.Empty;
            ExceptionType = string.Empty;
            HelpLink = string.Empty;
            BaseExceptionSourceInfo = CallerInfo.Empty;
            StackTraceFrames = [];
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ExceptionInfo"/> a partir de un objeto <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">La excepción original de la cual se extraerá la información.</param>
        /// <param name="depth">La profundidad de anidamiento inicial de esta excepción (usado para formateo).</param>
        public ExceptionInfo(Exception ex, int depth = 1) : this()
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));
            _currentExceptionDepth = depth;
            LoadExceptionDetails(ex, depth);
        }

        /// <summary>
        /// Obtiene la información adicional (clave-valor) asociada con la excepción.
        /// Corresponde a la colección <see cref="Exception.Data"/>.
        /// </summary>
        public List<KeyValuePair<string, string>> AdditionalInfo { get; private set; }

        /// <summary>
        /// Obtiene una lista de <see cref="ExceptionInfo"/> para cada excepción contenida en una <see cref="AggregateException"/>.
        /// </summary>
        public List<ExceptionInfo> AggregateExceptions { get; private set; }

        /// <summary>
        /// Obtiene un identificador único generado para la excepción (ej. un hash).
        /// </summary>
        public string ExceptionId { get; private set; }

        /// <summary>
        /// Obtiene el enlace a un archivo de ayuda asociado con esta excepción.
        /// </summary>
        public string HelpLink { get; private set; }

        /// <summary>
        /// Obtiene la información de la excepción interna (<see cref="Exception.InnerException"/>) como un objeto <see cref="ExceptionInfo"/>.
        /// </summary>
        public ExceptionInfo? InnerExceptionInfo { get; private set; }

        /// <summary>
        /// Obtiene el mensaje de la excepción base (<see cref="Exception.GetBaseException()"/>).
        /// </summary>
        public string BaseExceptionMessage { get; private set; }

        /// <summary>
        /// Obtiene el mensaje descriptivo de la excepción actual.
        /// </summary>
        public string ExceptionMessage { get; private set; }

        /// <summary>
        /// Obtiene el nombre de la aplicación o el objeto que causó el error.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Obtiene el método que originó la excepción.
        /// </summary>
        public string TargetSite { get; private set; }

        /// <summary>
        /// Obtiene el tipo de la excepción (ej. "NullReferenceException").
        /// </summary>
        public string ExceptionType { get; private set; }

        /// <summary>
        /// Obtiene la información del llamador (método, archivo, línea) de la excepción base.
        /// </summary>
        internal CallerInfo BaseExceptionSourceInfo { get; private set; }

        /// <summary>
        /// Obtiene la lista de marcos de la pila de llamadas (<see cref="StackTrace"/>) como objetos <see cref="CallerInfo"/>.
        /// </summary>
        internal List<CallerInfo> StackTraceFrames { get; private set; }

        /// <summary>
        /// Genera una cadena de texto que representa la información adicional de la excepción, formateada con indentación.
        /// </summary>
        /// <returns>Una cadena con la información adicional, o una cadena vacía si no hay información.</returns>
        public string GetAdditionalInfoBlock()
        {
            if (AdditionalInfo == null || AdditionalInfo.Count == 0) return string.Empty;

            var block = new StringBuilder();
            block.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine("Additional Info:");
            foreach (var item in AdditionalInfo)
            {
                block.Append(GetIndentString(2, _currentExceptionDepth)).AppendLine($"{item.Key} => {item.Value}");
            }
            return block.ToString();
        }

        /// <summary>
        /// Genera una cadena de texto para el enlace de ayuda, formateada con indentación.
        /// </summary>
        /// <returns>Una cadena con el enlace de ayuda, o una cadena vacía si no existe.</returns>
        public string GetHelpLinkBlock()
        {
            if (string.IsNullOrEmpty(HelpLink)) return string.Empty;

            var block = new StringBuilder();
            block.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine($"HelpLink: {HelpLink}");
            return block.ToString();
        }

        /// <summary>
        /// Genera un reporte completo y formateado de la excepción.
        /// </summary>
        /// <param name="includeStackTrace">Indica si se debe incluir la traza de la pila en el reporte.</param>
        /// <param name="includeAggregateExceptions">Indica si se deben incluir las excepciones agregadas en el reporte.</param>
        /// <returns>Una cadena de texto con el reporte detallado de la excepción.</returns>
        public string GetReport(bool includeStackTrace = true, bool includeAggregateExceptions = true)
        {
            var reportBuilder = new StringBuilder();
            reportBuilder.Append(GetTitleBlock());

            if (!string.IsNullOrEmpty(HelpLink)) reportBuilder.Append(GetHelpLinkBlock());
            if (AdditionalInfo != null && AdditionalInfo.Count > 0) reportBuilder.Append(GetAdditionalInfoBlock());

            // Solo añade una línea en blanco si hay información adicional o de ayuda para separar del stacktrace
            if (!string.IsNullOrEmpty(HelpLink) || (AdditionalInfo != null && AdditionalInfo.Count > 0))
            {
                reportBuilder.AppendLine();
            }


            if (includeStackTrace && StackTraceFrames != null && StackTraceFrames.Count > 0)
            {
                reportBuilder.Append(GetStackTraceBlock());
            }

            var currentInner = InnerExceptionInfo;
            if (currentInner != null)
            {
                reportBuilder.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine("InnerException:");
                // Bucle para procesar todas las excepciones internas anidadas
                while (currentInner != null)
                {
                    reportBuilder.Append(currentInner.GetReport(includeStackTrace, includeAggregateExceptions));
                    currentInner = currentInner.InnerExceptionInfo;
                }
            }

            if (includeAggregateExceptions && AggregateExceptions.Count > 0)
            {
                reportBuilder.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine("AggregateExceptions:");
                foreach (var aggExInfo in AggregateExceptions)
                {
                    reportBuilder.Append(aggExInfo.GetReport(includeStackTrace, includeAggregateExceptions));
                }
            }

            return reportBuilder.ToString();
        }

        /// <summary>
        /// Genera una cadena de texto que representa la traza de la pila, formateada con indentación.
        /// </summary>
        /// <returns>Una cadena con la traza de la pila, o una cadena vacía si no hay información.</returns>
        public string GetStackTraceBlock()
        {
            if (StackTraceFrames == null || StackTraceFrames.Count == 0) return string.Empty;

            var block = new StringBuilder();
            block.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine("StackTrace:");
            foreach (var frameInfo in StackTraceFrames)
            {
                // La indentación de cada línea del stacktrace es un nivel más profundo que el título "StackTrace:"
                // Se añade un tab explícito para consistencia con el código original.
                block.Append(GetIndentString(2, _currentExceptionDepth)).Append("\t").AppendLine(frameInfo.ToString());
            }
            return block.ToString();
        }

        /// <summary>
        /// Genera el bloque de título del reporte, incluyendo tipo, mensaje y origen de la excepción.
        /// </summary>
        /// <returns>Una cadena de texto con el título formateado de la excepción.</returns>
        public string GetTitleBlock()
        {
            var block = new StringBuilder();
            // El título principal de la excepción se indenta según la profundidad actual de la excepción.
            block.Append(GetIndentString(0, _currentExceptionDepth)).AppendLine($"{ExceptionType}: \"{ExceptionMessage}\"");

            if (!BaseExceptionSourceInfo.IsEmpty) // Asume que CallerInfo tiene una propiedad IsEmpty
            {
                // La información de origen se indenta un nivel más que el título.
                block.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine("Source (from BaseException):");
                block.Append(GetIndentString(2, _currentExceptionDepth)).Append("\t").AppendLine(BaseExceptionSourceInfo.ToString());
            }
            else if (!string.IsNullOrEmpty(Source)) // Si no hay BaseExceptionSourceInfo pero sí hay Source
            {
                block.Append(GetIndentString(1, _currentExceptionDepth)).AppendLine($"Source: {Source}");
            }
            return block.ToString();
        }

        /// <summary>
        /// Carga la información adicional (datos de la colección <see cref="Exception.Data"/>) de la excepción.
        /// </summary>
        /// <param name="ex">La excepción de la cual cargar la información.</param>
        private void LoadAdditionalData(Exception ex)
        {
            if (ex.Data == null || ex.Data.Count == 0) return;

            foreach (var key in ex.Data.Keys)
            {
                AdditionalInfo.Add(new KeyValuePair<string, string>(
                    key?.ToString() ?? "null_key",
                    ex.Data[key]?.ToString() ?? "null_value"));
            }
        }

        /// <summary>
        /// Carga las excepciones internas de una <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="aggEx">La <see cref="AggregateException"/> a procesar.</param>
        /// <param name="parentDepth">La profundidad de la excepción padre (AggregateException).</param>
        private void LoadAggregateExceptionDetails(AggregateException aggEx, int parentDepth)
        {
            // Cada excepción aplanada de AggregateException se considera en el mismo nivel de profundidad
            // que la AggregateException misma en términos de reporte, o un nivel más si se quiere anidar visualmente.
            // El código original usaba `exceptionDepth` que es `parentDepth` aquí.
            // Si se desea que cada excepción agregada tenga una indentación mayor, se pasaría `parentDepth + 1`.
            // Mantendré el comportamiento original: `parentDepth`
            foreach (var innerEx in aggEx.InnerExceptions)
            {
                AggregateExceptions.Add(new ExceptionInfo(innerEx, parentDepth));
            }
        }

        /// <summary>
        /// Carga toda la información relevante de la excepción proporcionada en esta instancia de <see cref="ExceptionInfo"/>.
        /// </summary>
        /// <param name="ex">La excepción a procesar.</param>
        /// <param name="depth">La profundidad actual de anidamiento de la excepción.</param>
        private void LoadExceptionDetails(Exception ex, int depth)
        {
            // Si la excepción es una AggregateException, se procesan sus excepciones internas.
            if (ex is AggregateException aggregateException)
            {
                // Aquí se decide si el mensaje de la AggregateException misma es el principal
                // o si se debe omitir en favor de sus InnerExceptions.
                // El código original procesaba las propiedades de la AggregateException y luego sus internas.
                LoadAggregateExceptionDetails(aggregateException, depth);
            }

            HelpLink = ex.HelpLink ?? string.Empty;
            ExceptionMessage = ex.Message ?? string.Empty;
            Source = ex.Source ?? string.Empty;
            TargetSite = ex.TargetSite?.ToString() ?? string.Empty;
            ExceptionType = ex.GetType().Name;

            // Recursivamente carga la InnerException, incrementando la profundidad.
            if (ex.InnerException != null)
            {
                InnerExceptionInfo = new ExceptionInfo(ex.InnerException, depth + 1);
            }

            // Genera un identificador para la excepción
            ExceptionId = ex.GetExceptionHash();

            LoadBaseExceptionSource(ex.GetBaseException());
            LoadStackTraceInfo(ex);
            LoadAdditionalData(ex);
        }

        /// <summary>
        /// Carga la información de origen de la excepción base (la causa raíz).
        /// </summary>
        /// <param name="baseEx">La excepción base obtenida de <see cref="Exception.GetBaseException()"/>.</param>
        private void LoadBaseExceptionSource(Exception? baseEx) // Permite que baseEx sea null
        {
            BaseExceptionSourceInfo = CallerInfo.Empty; // Valor predeterminado
            if (baseEx == null) return;

            BaseExceptionMessage = baseEx.Message ?? string.Empty; // Mensaje de la excepción base

            // Solo intenta obtener el StackTrace si baseEx no es null.
            // Un try-catch podría ser útil aquí si la creación de StackTrace o GetFrame pudiera fallar inesperadamente,
            // aunque generalmente es seguro.
            try
            {
                var stackTrace = new StackTrace(baseEx, true);
                var frame = stackTrace.GetFrame(0); // Obtiene el marco superior de la pila de la excepción base
                if (frame != null)
                {
                    BaseExceptionSourceInfo = new CallerInfo(frame); // Asume constructor de CallerInfo
                }
            }
            catch (Exception ex)
            {
                // Error al obtener la información de la pila para la excepción base.
                // Se podría registrar este error o añadir información a AdditionalInfo.
                // Por ahora, BaseExceptionSourceInfo permanecerá como CallerInfo.Empty.
                Debug.WriteLine($"Error loading base exception stack trace: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga la traza de la pila de la excepción.
        /// </summary>
        /// <param name="ex">La excepción de la cual cargar la traza de la pila.</param>
        private void LoadStackTraceInfo(Exception ex)
        {
            // Un try-catch podría ser útil si la creación de StackTrace o GetFrames pudiera fallar,
            // aunque generalmente es seguro.
            try
            {
                var stackTrace = new StackTrace(ex, true); // 'true' para capturar información de archivo y línea

                // GetFrames() puede devolver null si no hay marcos de pila.
                var frames = stackTrace.GetFrames();
                if (frames != null)
                {
                    // Revierte los marcos para que el origen de la excepción aparezca primero (más profundo en la pila)
                    // y el punto de captura más reciente al final, o viceversa según se prefiera.
                    // El código original usaba Reverse(), lo que significa que el error más profundo (origen) está al final.
                    // Usualmente se muestra desde el punto de catch hacia el origen. Mantendré el Reverse().
                    StackTraceFrames = [.. frames.Select(frame => new CallerInfo(frame)).Reverse()];
                }
                else
                {
                    StackTraceFrames = [];
                }
            }
            catch (Exception e)
            {
                // Error al obtener la información de la pila.
                Debug.WriteLine($"Error loading stack trace: {e.Message}");
                StackTraceFrames = []; // Asegurar que sea una lista vacía
            }
        }

        /// <summary>
        /// Genera una cadena de espacios para la indentación basada en la profundidad estructural y la profundidad de la excepción.
        /// </summary>
        /// <param name="structureIndentLevel">Nivel de indentación para el elemento estructural dentro del reporte de esta excepción (ej. 0 para título, 1 para "Source", 2 para items).</param>
        /// <param name="currentExceptionDepth">Profundidad de anidamiento de la instancia actual de <see cref="ExceptionInfo"/>.</param>
        /// <returns>Una cadena de espacios para la indentación.</returns>
        private static string GetIndentString(int structureIndentLevel, int currentExceptionDepth)
        {
            // La indentación total es la suma de la profundidad de la excepción
            // (para anidar reportes de InnerException/AggregateException)
            // y la indentación estructural dentro del reporte de la excepción actual.
            // Restamos 1 de currentExceptionDepth porque una profundidad de 1 no debería indentar el título principal.
            int totalDepth = Math.Max(0, currentExceptionDepth - 1) + structureIndentLevel;
            return new string(' ', IndentWidth * totalDepth);
        }

    }
}
