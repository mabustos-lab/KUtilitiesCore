using KUtilitiesCore.Dal.Exceptions;
using KUtilitiesCore.Dal.Helpers;
using KUtilitiesCore.Data;
using KUtilitiesCore.Data.ValidationAttributes;
using KUtilitiesCore.Extensions;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace KUtilitiesCore.Dal.ConnectionBuilder
{
    /// <summary>
    /// Clase base contructor de cadena de conexiones
    /// </summary>
    public abstract class ConnectionBuilderBase : IConnectionBuilder, INotifyPropertyChanged
    {
        #region Fields

        internal string applicationName;

        internal int connectionTimeout = 30;

        internal bool encrypt;

        internal string initialCatalog;

        internal bool integratedSecurity;

        internal string password;

        internal string providerName;

        internal string serverName;

        internal bool trustServerCertificate;

        internal string userName;

        #endregion Fields

        #region Events

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events
        
        #region Properties

        /// <inheritdoc/>
        [JsonProperty("AN")]
        [JsonPropertyName("AN")]
        public virtual string ApplicationName
        {
            get => applicationName;
            set => this.SetValueAndNotify(ref applicationName, value);
        }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual string CnnString { get => GetCnnString(); }

        /// <inheritdoc/>
        [JsonProperty("CT")]
        [JsonPropertyName("CT")]
        public virtual int ConnectionTimeout
        {
            get => connectionTimeout;
            set => this.SetValueAndNotify(ref connectionTimeout, value);
        }

        /// <summary>
        /// Establece la configuración default por el usuario para la conección a la base de datos.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Action<IConnectionBuilder> CustomDefaultConfig { get; set; }

        /// <inheritdoc/>
        [JsonProperty("E")]
        [JsonPropertyName("E")]
        public virtual bool Encrypt { get => encrypt; set => this.SetValueAndNotify(ref encrypt, value); }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Error => string.Empty;

        /// <inheritdoc/>
        [JsonProperty("IC")]
        [JsonPropertyName("IC")]
        [Required(AllowEmptyStrings = false)]
        public virtual string InitialCatalog
        {
            get => initialCatalog;
            set => this.SetValueAndNotify(ref initialCatalog, value);
        }

        /// <inheritdoc/>
        [JsonProperty("IS")]
        [JsonPropertyName("IS")]
        public virtual bool IntegratedSecurity
        {
            get => integratedSecurity;
            set => this.SetValueAndNotify(ref integratedSecurity, value);
        }

        /// <inheritdoc/>
        [JsonProperty("P")]
        [JsonPropertyName("P")]
        [RequiredIf("IntegratedSecurity", false)]
        public virtual string Password { get => password; set => this.SetValueAndNotify(ref password, value); }

        /// <inheritdoc/>
        [JsonProperty("PN")]
        [JsonPropertyName("PN")]
        [Required(AllowEmptyStrings = false)]
        public string ProviderName { get => providerName; set => this.SetValueAndNotify(ref providerName, value); }

        /// <inheritdoc/>
        [JsonProperty("SN")]
        [JsonPropertyName("SN")]
        [Required(AllowEmptyStrings = false)]
        public virtual string ServerName { get => serverName; set => this.SetValueAndNotify(ref serverName, value); }

        /// <inheritdoc/>
        [JsonProperty("TSC")]
        [JsonPropertyName("TSC")]
        public virtual bool TrustServerCertificate
        {
            get => trustServerCertificate;
            set => this.SetValueAndNotify(ref trustServerCertificate, value);
        }

        /// <inheritdoc/>
        [JsonProperty("UID")]
        [JsonPropertyName("UID")]
        [RequiredIfAttribute("IntegratedSecurity", false)]
        public virtual string UserName { get => userName; set => this.SetValueAndNotify(ref userName, value); }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Obtiene el mensaje de error de validación para la propiedad especificada.
        /// </summary>
        /// <param name="columnName">Nombre de la propiedad.</param>
        /// <returns>Mensaje de error o cadena vacía si no hay error.</returns>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual string this[string columnName] => this.GetErrorText(columnName);

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Indica si existen los datos mínimos para listar las bases de datos del servidor.
        /// </summary>
        /// <returns>True si se puede listar, false en caso contrario.</returns>
        public virtual bool CanListDatabases() => !string.IsNullOrEmpty(ServerName) &&
            !string.IsNullOrEmpty(ProviderName) &&
            (IntegratedSecurity || (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)));

        /// <summary>
        /// Indica si la configuración actual es válida según los atributos de validación.
        /// </summary>
        /// <returns>True si es válida, false en caso contrario.</returns>
        public virtual bool IsValid() => !DataErrorInfoExt.HasErrors(this);

        /// <summary>
        /// Obtiene un <see cref="DataTable"/> con la lista de bases de datos disponibles en el servidor.
        /// </summary>
        /// <returns>DataTable con columnas: DatabaseName, Owner, CreatedDate.</returns>
        /// <exception cref="DataAccessException">
        /// Si ocurre un error al listar las bases de datos.
        /// </exception>
        public virtual DataTable ListDatabases()
        {
            DataTable dt = new DataTable();
            if (CanListDatabases())
            {
                try
                {
                    using DaoContext ctx = new DaoContext(this);
                    // Obtener los catálogos (bases de datos)
                    DataTable schemaTable = ctx.Connection.GetSchema("Databases");

                    // Normalizar nombres de columnas para diferentes proveedores
                    dt.Columns.Add("DatabaseName", typeof(string));
                    dt.Columns.Add("Owner", typeof(string));
                    dt.Columns.Add("CreatedDate", typeof(DateTime));
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        DataRow newRow = dt.NewRow();
                        newRow["DatabaseName"] = row.Table.Columns.Contains("database_name")
                            ? row["database_name"]
                            : row["DATABASE_NAME"];
                        newRow["Owner"] = row.Table.Columns.Contains("owner") ? row["owner"] : DBNull.Value;
                        newRow["CreatedDate"] = row.Table.Columns.Contains("created") ? row["created"] : DBNull.Value;
                        dt.Rows.Add(newRow);
                    }
                }
                catch (Exception ex)
                {
                    throw new DataAccessException("Error al listar las bases de datos.", "GetSchema", null, ex);
                }
            }
            return dt;
        }

        /// <summary>
        /// Notifica el cambio de una propiedad.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad.</param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        /// <summary>
        /// Restablece los valores de la configuración a los valores predeterminados.
        /// </summary>
        public virtual void Reset()
        {
            ApplicationName = "MyApp";
            InitialCatalog = "master";
            IntegratedSecurity = false;
            Password = string.Empty;
#if NET48
            ProviderName = "System.Data.SqlClient";
#else
            ProviderName = "Microsoft.Data.SqlClient";
#endif
            ProviderName = "System.Data.SqlClient";
            ServerName = "Localhost";
            UserName = string.Empty;
            Encrypt = true;
            TrustServerCertificate = true;
            if (CustomDefaultConfig != null)
                CustomDefaultConfig(this);
        }

        /// <summary>
        /// Realiza una prueba de conexión a la base de datos y devuelve el resultado.
        /// </summary>
        /// <returns>Un objeto <see cref="TestConnectionResult"/> con el resultado de la prueba.</returns>
        public virtual TestConnectionResult TestConnection()
        {
            TestConnectionResult ret = TestConnectionResult.SucessTest;
            try
            {
                using IDaoContext ctx = new DaoContext(this);
                ret.PublishedServerName = ctx.GetPublishedServerNameAsync()
                    .ConfigureAwait(true)
                    .GetAwaiter()
                    .GetResult();
                ret.ServerVersion = ctx.Connection.ServerVersion;
            }
            catch (System.Exception ex)
            {
                if (!ex.Data.Contains("Server"))
                    ex.Data.Add("Server", ServerName);
                if (!ex.Data.Contains("Database"))
                    ex.Data.Add("Database", InitialCatalog);
                ret = TestConnectionResult.
                    FailTest(
                        new DataAccessException("No se pudo abrir la conexion a la Base de datos.", null, null, ex));
            }
            return ret;
        }

        /// <summary>
        /// Construye la cadena de conexión a partir de las propiedades actuales.
        /// </summary>
        /// <returns>Cadena de conexión resultante.</returns>
        internal string GetCnnString()
        {
            string res;
            try
            {
                DbConnectionStringBuilder dbcsb = new DbConnectionStringBuilder
                {
                    ["Application Name"] = ApplicationName,
                    ["Initial Catalog"] = InitialCatalog,
                    ["Data Source"] = ServerName,
                    ["Integrated Security"] = IntegratedSecurity
                };

                if (!IntegratedSecurity)
                {
                    dbcsb["User ID"] = UserName;
                    dbcsb["Password"] = Password;
                }

                dbcsb["Encrypt"] = Encrypt;
                dbcsb["TrustServerCertificate"] = TrustServerCertificate;
                dbcsb["Connection Timeout"] = ConnectionTimeout;
                // Añadir otros parámetros si son necesarios (e.g., Connection Timeout, Pooling)
                // sb.Append("Connection Timeout=30;");
                res = dbcsb.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un problema: {ex.Message}");
                throw;
            }

            return res;
        }
        /// <summary>
        /// Establece los paramentros a partir de una cadena de conexión.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public void SetCnnStringProperties(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

            try
            {
                var dbcsb = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                // 1. Application Name
                if (TryGetAnyValue(dbcsb, out var appName, "Application Name", "ApplicationName", "App"))
                    ApplicationName = appName?.ToString();
                else
                    ApplicationName = "MyApp"; // Default

                // 2. Server / Data Source
                if (TryGetAnyValue(dbcsb, out var server, "Data Source", "Server", "Server Name", "Address", "Addr", "Network Address"))
                    ServerName = server?.ToString();
                else
                    ServerName = "Localhost"; // Default

                // 3. Database / Initial Catalog
                if (TryGetAnyValue(dbcsb, out var database, "Initial Catalog", "InitialCatalog", "Database", "Database Name", "DatabaseName", "DB"))
                    InitialCatalog = database?.ToString();
                else
                    InitialCatalog = "master"; // Default

                // 4. Seguridad Integrada
                // Nota: Algunos proveedores usan "Trusted_Connection=yes" o "Integrated Security=SSPI"
                if (TryGetAnyValue(dbcsb, out var integratedSec, "Integrated Security", "IntegratedSecurity", "Trusted Connection", "Trusted_Connection"))
                {
                    // Manejo especial para "SSPI" que es común en SQL Server pero falla en Convert.ToBoolean
                    string secVal = integratedSec?.ToString() ?? "false";
                    IntegratedSecurity = secVal.Equals("SSPI", StringComparison.OrdinalIgnoreCase) ||
                                         ParseBooleanSafe(secVal);
                }
                else
                {
                    IntegratedSecurity = false;
                }

                // 5. Credenciales
                if (TryGetAnyValue(dbcsb, out var user, "User ID", "UserID", "UID", "User", "User Name", "UserName"))
                    UserName = user?.ToString();
                else
                    UserName = string.Empty;

                if (TryGetAnyValue(dbcsb, out var pwd, "Password", "Pwd", "Secret"))
                    Password = pwd?.ToString();
                else
                    Password = string.Empty;

                // 6. Configuración adicional (Encrypt, Trust, Timeout)
                if (TryGetAnyValue(dbcsb, out var encryptVal, "Encrypt", "Encryption"))
                    Encrypt = ParseBooleanSafe(encryptVal);
                else
                    Encrypt = true;

                if (TryGetAnyValue(dbcsb, out var trustVal, "TrustServerCertificate", "Trust Server Certificate", "TrustServerCert"))
                    TrustServerCertificate = ParseBooleanSafe(trustVal);
                else
                    TrustServerCertificate = true;

                if (TryGetAnyValue(dbcsb, out var timeoutVal, "Connection Timeout", "Connect Timeout", "ConnectionTimeout", "ConnectTimeout", "Timeout"))
                {
                    if (int.TryParse(timeoutVal?.ToString(), out int timeout))
                        ConnectionTimeout = timeout;
                    else
                        ConnectionTimeout = 30;
                }
                else
                {
                    ConnectionTimeout = 30;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al parsear cadena de conexión: {ex.Message}");
                throw new ArgumentException("La estructura de la cadena de conexión no es válida.", nameof(connectionString), ex);
            }
        }

        /// <summary>
        /// Intenta obtener un valor del builder buscando por múltiples claves (sinónimos).
        /// </summary>
        private bool TryGetAnyValue(DbConnectionStringBuilder builder, out object value, params string[] keys)
        {
            value = null;
            foreach (var key in keys)
            {
                if (builder.TryGetValue(key, out value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convierte valores string a boolean de forma segura (ej. "yes", "true", "1").
        /// </summary>
        private bool ParseBooleanSafe(object value)
        {
            if (value == null) return false;
            string valStr = value.ToString();

            if (bool.TryParse(valStr, out bool result)) return result;

            // Soporte para "yes"/"no" o "1"/"0" que a veces aparecen en cadenas legacy
            if (valStr.Equals("yes", StringComparison.OrdinalIgnoreCase) || valStr == "1") return true;

            return false;
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Obtiene la lista de nombres invariantes de proveedores registrados en el sistema. Solo
        /// disponible en .NET 8 o superior.
        /// </summary>
        /// <returns>Enumeración de nombres invariantes de proveedores.</returns>
        public static IEnumerable<string> GetProviderInvariantNames()
        { return DbProviderFactories.GetProviderInvariantNames(); }
#endif
        #endregion Methods
    }
}