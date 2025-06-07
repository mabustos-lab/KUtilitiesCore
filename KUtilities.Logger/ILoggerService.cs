using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Interfaz para un servicio de logging genérico.
    /// </summary>
    public interface ILoggerService: ILogger
    {
        /// <summary>
        /// Formatea y escribe un mensaje de log de Trace.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogTrace(string message, EventId? eventId, params object[] args);
        void LogTrace(Exception exception, string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Debug.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogDebug(string message, EventId? eventId, params object[] args);
        void LogDebug(Exception exception, string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Information.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogInformation(string message, EventId? eventId, params object[] args);
        void LogInformation(Exception exception, string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Warning.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogWarning(string message, EventId? eventId, params object[] args);
        void LogWarning(Exception exception, string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Error.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogError(string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Error.
        /// </summary>
        /// <param name="exception">La excepción a registrar.</param>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogError(Exception exception, string message, EventId? eventId, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Critical.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogCritical(string message, EventId? eventId, params object[] args);
        void LogCritical(Exception exception, string message, EventId? eventId, params object[] args);
    }
    /// <summary>
    /// Interfaz para un servicio de logging genérico.
    /// Abstrae la implementación de logging específica (<see cref="ILogger{T}"/>) para facilitar
    /// la inyección de dependencias y las pruebas unitarias.
    /// </summary>
    public interface ILoggerService<out TCategory>:ILogger<TCategory>, ILoggerService
    {        
    }
}
