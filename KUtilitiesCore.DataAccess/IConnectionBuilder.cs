using KUtilitiesCore.Data.ValidationAttributes;
using KUtilitiesCore.DataAccess.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;

namespace KUtilitiesCore.DataAccess
{
    /// <summary>
    /// Interfaz para la construcción de cadenas de conexión independiente de la infraestructura de datos.
    /// </summary>
    public interface IConnectionBuilder : IConnectionString, IDataErrorInfo
    {
        /// <summary>
        /// Regresa un DataTable con la lista de las bases de datos disponibles en el Servidor
        /// </summary>
        /// <returns>Regresa un DataTable con las columnas: Owner, DatabaseName y CreatedDate</returns>
        DataTable ListDatabases();

        /// <summary>
        /// Indica si existe la configuración minima para listar las Bases de datos
        /// </summary>
        /// <returns>True si se puede listar las bases de datos, de lo contrario false.</returns>
        bool CanListDatabases();

        /// <summary>
        /// Establece o obtiene el nombre de la aplicación que realiza la conexión.
        /// </summary>
        [JsonProperty("AN")]
        [JsonPropertyName("AN")]
        string ApplicationName { get; set; }

        /// <summary>
        /// Indica si se utiliza cifrado SSL en la conexión.
        /// </summary>
        [JsonProperty("E")]
        [JsonPropertyName("E")]
        bool Encrypt { get; set; }

        /// <summary>
        /// Establece o obtiene el nombre del catálogo inicial, es decir, el nombre de la base de datos.
        /// </summary>
        [JsonProperty("IC")]
        [JsonPropertyName("IC")]
        [Required(AllowEmptyStrings = false)]
        string InitialCatalog { get; set; }

        /// <summary>
        /// Obtiene o establece un valor booleano que indica si se utilizan las credenciales actuales de la cuenta de Windows
        /// para la autenticación (cuando es true) o si se especifican el identificador de usuario y la contraseña en la
        /// conexión (cuando es false).
        /// </summary>
        [JsonProperty("IS")]
        [JsonPropertyName("IS")]
        bool IntegratedSecurity { get; set; }

        /// <summary>
        /// Establece o obtiene la contraseña para la conexión a la base de datos.
        /// </summary>
        [JsonProperty("P")]
        [JsonPropertyName("P")]
        string Password { get; set; }

        /// <summary>
        /// Establece o obtiene el nombre o la dirección IP del servidor donde se encuentra la base de datos.
        /// </summary>
        [JsonProperty("SN")]
        [JsonPropertyName("SN")]
        [Required(AllowEmptyStrings = false)]
        string ServerName { get; set; }
        /// <summary>
        /// Obtiene o establece el tiempo máximo, en segundos, para esperar a que se establezca una conexión.
        /// </summary>
        [JsonProperty("CT")]
        [JsonPropertyName("CT")]
        int ConnectionTimeout { get;set; }
        /// <summary>
        /// Indica si se confía en el certificado del servidor.
        /// </summary>
        [JsonProperty("TSC")]
        [JsonPropertyName("TSC")]
        bool TrustServerCertificate { get; set; }

        /// <summary>
        /// Establece o obtiene el nombre de usuario para la conexión a la base de datos.
        /// </summary>
        [JsonProperty("UID")]
        [JsonPropertyName("UID")]
        [RequiredIf("IntegratedSecurity", false)]
        string UserName { get; set; }

        /// <summary>
        /// Carga los valores almacenados en el objeto.
        /// </summary>
        void Load();

        /// <summary>
        /// Restaura la cadena de conexión a su valor predeterminado.
        /// </summary>
        void Reset();

        /// <summary>
        /// Almacena los cambios realizados en el objeto.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Realiza una prueba de conexión a la base de datos y devuelve un resultado.
        /// </summary>
        /// <returns>
        /// Un objeto <see cref="TestConnectionResult"/> que contiene el resultado de la prueba de conexión.
        /// </returns>
        TestConnectionResult TestConnection();
    }
}