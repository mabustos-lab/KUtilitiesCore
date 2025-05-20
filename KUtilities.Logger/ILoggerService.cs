using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Interfaz para un servicio de logging genérico.
    /// Abstrae la implementación de logging específica (ILogger<T>) para facilitar
    /// la inyección de dependencias y las pruebas unitarias.
    /// </summary>
    /// <typeparam name="TCategoryName">El tipo de la clase que realiza el logging.</typeparam>
    public interface ILoggerService<TCategoryName>
    {
        /// <summary>
        /// Escribe un mensaje de log con el nivel de severidad especificado.
        /// </summary>
        /// <param name="logLevel">Nivel de entrada.</param>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="exception">La excepción a registrar.</param>
        /// <param name="message">Formato de cadena del mensaje. Puede contener placeholders para argumentos.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args);

        /// <summary>
        /// Escribe un mensaje de log con el nivel de severidad especificado.
        /// </summary>
        /// <param name="logLevel">Nivel de entrada.</param>
        /// <param name="eventId">Id del evento.</param>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void Log(LogLevel logLevel, EventId eventId, string message, params object[] args);

        /// <summary>
        /// Escribe un mensaje de log con el nivel de severidad especificado.
        /// </summary>
        /// <param name="logLevel">Nivel de entrada.</param>
        /// <param name="exception">La excepción a registrar.</param>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void Log(LogLevel logLevel, Exception exception, string message, params object[] args);

        /// <summary>
        /// Escribe un mensaje de log con el nivel de severidad especificado.
        /// </summary>
        /// <param name="logLevel">Nivel de entrada.</param>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void Log(LogLevel logLevel, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Trace.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogTrace(string message, params object[] args);
        void LogTrace(Exception exception, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Debug.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogDebug(string message, params object[] args);
        void LogDebug(Exception exception, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Information.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogInformation(string message, params object[] args);
        void LogInformation(Exception exception, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Warning.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogWarning(string message, params object[] args);
        void LogWarning(Exception exception, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Error.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Error.
        /// </summary>
        /// <param name="exception">La excepción a registrar.</param>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogError(Exception exception, string message, params object[] args);

        /// <summary>
        /// Formatea y escribe un mensaje de log de Critical.
        /// </summary>
        /// <param name="message">Formato de cadena del mensaje.</param>
        /// <param name="args">Un array de objetos con los argumentos para el mensaje.</param>
        void LogCritical(string message, params object[] args);
        void LogCritical(Exception exception, string message, params object[] args);

        /// <summary>
        /// Verifica si el nivel de log especificado está habilitado.
        /// </summary>
        /// <param name="logLevel">Nivel de log a verificar.</param>
        /// <returns>true si está habilitado, false en caso contrario.</returns>
        bool IsEnabled(LogLevel logLevel);
    }
}
