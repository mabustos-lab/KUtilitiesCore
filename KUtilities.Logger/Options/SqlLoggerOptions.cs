using Microsoft.Extensions.Logging;
using System;

namespace KUtilitiesCore.Logger.Options
{
    /// <summary>
    /// Opciones de configuración para el logger de SQL Server.
    /// </summary>
    public class SqlLoggerOptions : LoggerOptions
    {
        /// <summary>
        /// Cadena de conexión a la base de datos SQL Server.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la tabla donde se almacenarán los logs.
        /// </summary>
        public string TableName { get; set; } = "ApplicationLogs";

        /// <summary>
        /// Esquema de la base de datos (por defecto dbo).
        /// </summary>
        public string SchemaName { get; set; } = "dbo";

        /// <summary>
        /// Indica si se debe intentar crear la tabla automáticamente si no existe.
        /// </summary>
        public bool AutoCreateTable { get; set; } = true;

        /// <summary>
        /// Nombre de la aplicación para identificar el origen de los logs.
        /// </summary>
        public string ApplicationName { get; set; } = "KUtilitiesApp";
        /// <summary>
        /// Obtiene o establece el delegado utilizado para resolver el identificador del usuario actual.
        /// </summary>
        /// <remarks>
        /// El delegado debe devolver una cadena que represente al usuario, como un nombre de usuario o un ID de usuario.
        /// Esta propiedad se puede configurar para personalizar la forma en que se recupera la información del usuario, por ejemplo, para integrarla con
        /// sistemas de autenticación o proveedores de contexto de usuario.
        /// </remarks>
        public Func<string>? ResolveUser { get; set; }
    }
}
