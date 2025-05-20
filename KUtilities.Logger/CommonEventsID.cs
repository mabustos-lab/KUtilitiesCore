using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger
{
    /// <summary>
    /// Identificadores comunes de eventos para su uso en el sistema de logging.
    /// Cada propiedad representa un tipo de evento estándar que puede ser registrado,
    /// facilitando la categorización y el filtrado de logs.
    /// </summary>
    public static class CommonEventsID
    {
        /// <summary>
        /// Evento para pruebas.
        /// Usado para auditar resultados en pruebas unitarias.
        /// </summary>
        public static EventId Test => new EventId(999, "Test");
        /// <summary>
        /// Evento informativo general de la aplicación.
        /// Útil para registrar información relevante sobre el estado o acciones normales.
        /// </summary>
        public static EventId ApplicationInfo => new EventId(100, "AppInfo");

        /// <summary>
        /// Evento de error de la aplicación.
        /// Se utiliza para registrar errores o excepciones que ocurren durante la ejecución.
        /// </summary>
        public static EventId ApplicationError => new EventId(101, "AppError");

        /// <summary>
        /// Evento que indica el inicio de la aplicación.
        /// Permite registrar cuándo la aplicación ha comenzado su ejecución.
        /// </summary>
        public static EventId ApplicationStart => new EventId(102, "AppStart");

        /// <summary>
        /// Evento que indica la detención de la aplicación.
        /// Útil para registrar cuándo la aplicación finaliza su ejecución.
        /// </summary>
        public static EventId ApplicationStop => new EventId(103, "AppStop");

        /// <summary>
        /// Evento relacionado con acciones realizadas por el usuario.
        /// Permite auditar o rastrear interacciones de usuarios con el sistema.
        /// </summary>
        public static EventId UserAction => new EventId(110, "UserAction");

        /// <summary>
        /// Evento relacionado con el acceso a datos.
        /// Se utiliza para registrar operaciones de lectura o escritura en fuentes de datos.
        /// </summary>
        public static EventId DataAccess => new EventId(120, "DataAccess");

        /// <summary>
        /// Evento de error de validación.
        /// Útil para registrar fallos en la validación de datos de entrada o reglas de negocio.
        /// </summary>
        public static EventId ValidationError => new EventId(130, "ValidationError");

        /// <summary>
        /// Evento de alerta de seguridad.
        /// Se utiliza para registrar posibles amenazas o incidentes de seguridad detectados.
        /// </summary>
        public static EventId SecurityAlert => new EventId(140, "SecurityAlert");

        /// <summary>
        /// Evento relacionado con la configuración de la aplicación.
        /// Permite registrar cambios o problemas en la configuración.
        /// </summary>
        public static EventId Configuration => new EventId(150, "Configuration");

        /// <summary>
        /// Evento relacionado con el rendimiento de la aplicación.
        /// Útil para registrar métricas o incidencias de performance.
        /// </summary>
        public static EventId Performance => new EventId(160, "Performance");
    }
}
