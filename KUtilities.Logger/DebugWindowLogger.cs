using KUtilitiesCore.Logger.Helpers;
using KUtilitiesCore.Logger.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Logger especializado que escribe los mensajes en la ventana de salida de depuración de Visual Studio.
    /// Utiliza <see cref="System.Diagnostics.Debug.WriteLine(string)"/> para mostrar los logs en tiempo de desarrollo.
    /// Incluye información detallada de excepciones utilizando <see cref="ExceptionInfo"/>.
    /// </summary>
    /// <typeparam name="TCategoryName">El tipo de la clase que realiza el logging.</typeparam>
    /// <remarks>
    /// Inicializa una nueva instancia de <see cref="DebugWindowLogger{TCategoryName}"/>.
    /// </remarks>
    /// <param name="options">Opciones de configuración del logger.</param>
    public class DebugWindowLogger<TCategoryName>(LoggerOptions options) : LoggerServiceAbstract<TCategoryName, LoggerOptions>(options)
    {

        /// <summary>
        /// Escribe una entrada de log en la ventana de depuración.
        /// Si la entrada contiene una excepción, también escribe un reporte detallado de la misma.
        /// </summary>
        /// <param name="entry">La entrada de log a registrar.</param>
        internal override void WriteLog(LogEntry entry)
        {
            Debug.WriteLine(entry.ToString());
            if (entry.Exception != null)
            {
                var exception = new ExceptionInfo(entry.Exception);
                Debug.WriteLine(exception.GetReport());
            }
        }
    }
}
