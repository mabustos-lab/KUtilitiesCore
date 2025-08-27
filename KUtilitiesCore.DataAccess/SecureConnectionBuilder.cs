using KUtilitiesCore.Data;
using KUtilitiesCore.Data.ValidationAttributes;
using KUtilitiesCore.DataAccess.DAL;
using KUtilitiesCore.DataAccess.Helpers;
using KUtilitiesCore.DataAccess.Utils;
using KUtilitiesCore.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace KUtilitiesCore.DataAccess
{
    /// <summary>
    /// Proporciona una implementación segura de <see cref="IConnectionBuilder"/> que permite
    /// construir, almacenar y recuperar cadenas de conexión a bases de datos de forma cifrada.
    /// Incluye soporte para notificación de cambios de propiedades y validación de datos.
    /// </summary>
    public class SecureConnectionBuilder : IConnectionBuilder, INotifyPropertyChanged
    {
        /// <summary>
        /// Servicio de cifrado utilizado para proteger los datos sensibles.
        /// </summary>
        private readonly Encryption.IEncryptionService _encryptionService;

        /// <summary>
        /// Ruta del archivo donde se almacena la configuración cifrada.
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Constructor vacio para la Deserialización de la clase
        /// </summary>
        public SecureConnectionBuilder() : this(null, string.Empty)
        { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SecureConnectionBuilder"/>.
        /// </summary>
        /// <param name="encryptionService">
        /// Servicio de cifrado a utilizar. Si es nulo, se utiliza el servicio AES por defecto.
        /// </param>
        /// <param name="filePath">
        /// Ruta del archivo de configuración. Si es vacío, se utiliza la ruta predeterminada.
        /// </param>
        public SecureConnectionBuilder(Encryption.IEncryptionService encryptionService = null, string filePath = "")
        {
            Reset();
            encryptionService ??= Encryption.FactoryEncryptionService.GetAesEncryptionService("@!KUtilities0000");
            _encryptionService = encryptionService;
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(
                    IO.StoreFolder.GetSpecialStoreFolder(IO.SpecialStoreFolder.AllUserPublic),
                    "KUtilitiesCore",
                    "SecureConnection.enc");
            _filePath = filePath;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private string applicationName;
        private bool encrypt;
        private string initialCatalog;
        private bool integratedSecurity;
        private string password;
        private string providerName;
        private string serverName;
        private bool trustServerCertificate;
        private string userName;
        private int connectionTimeout = 30;

        /// <inheritdoc/>
        [JsonProperty("AN")]
        [JsonPropertyName("AN")]
        public string ApplicationName
        { get => applicationName; set => this.SetValueAndNotify(ref applicationName, value); }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CnnString { get => GetCnnString(); }

        /// <inheritdoc/>
        [JsonProperty("E")]
        [JsonPropertyName("E")]
        public bool Encrypt
        { get => encrypt; set => this.SetValueAndNotify(ref encrypt, value); }

        /// <inheritdoc/>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Error => string.Empty;

        /// <inheritdoc/>
        [JsonProperty("IC")]
        [JsonPropertyName("IC")]
        [Required(AllowEmptyStrings = false)]
        public string InitialCatalog
        { get => initialCatalog; set => this.SetValueAndNotify(ref initialCatalog, value); }

        /// <inheritdoc/>
        [JsonProperty("IS")]
        [JsonPropertyName("IS")]
        public bool IntegratedSecurity
        { get => integratedSecurity; set => this.SetValueAndNotify(ref integratedSecurity, value); }

        /// <inheritdoc/>
        [JsonProperty("P")]
        [JsonPropertyName("P")]
        [RequiredIf("IntegratedSecurity", false)]
        public string Password { get => password; set => this.SetValueAndNotify(ref password, value); }

        /// <inheritdoc/>
        [JsonProperty("PN")]
        [JsonPropertyName("PN")]
        [Required(AllowEmptyStrings = false)]
        public string ProviderName { get => providerName; set => this.SetValueAndNotify(ref providerName, value); }

        /// <inheritdoc/>
        [JsonProperty("SN")]
        [JsonPropertyName("SN")]
        [Required(AllowEmptyStrings = false)]
        public string ServerName { get => serverName; set => this.SetValueAndNotify(ref serverName, value); }

        /// <inheritdoc/>
        [JsonProperty("TSC")]
        [JsonPropertyName("TSC")]
        public bool TrustServerCertificate
        { get => trustServerCertificate; set => this.SetValueAndNotify(ref trustServerCertificate, value); }

        /// <inheritdoc/>
        [JsonProperty("UID")]
        [JsonPropertyName("UID")]
        [RequiredIfAttribute("IntegratedSecurity", false)]
        public string UserName { get => userName; set => this.SetValueAndNotify(ref userName, value); }

        /// <inheritdoc/>
        [JsonProperty("CT")]
        [JsonPropertyName("CT")]
        public int ConnectionTimeout
        { get => connectionTimeout; set => this.SetValueAndNotify(ref connectionTimeout, value); }

        /// <summary>
        /// Obtiene el mensaje de error de validación para la propiedad especificada.
        /// </summary>
        /// <param name="columnName">Nombre de la propiedad.</param>
        /// <returns>Mensaje de error o cadena vacía si no hay error.</returns>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string this[string columnName] => this.GetErrorText(columnName);

        /// <summary>
        /// Indica si la configuración actual es válida según los atributos de validación.
        /// </summary>
        /// <returns>True si es válida, false en caso contrario.</returns>
        public bool IsValid() => !DataErrorInfoExt.HasErrors(this);

        /// <summary>
        /// Carga la configuración cifrada desde el archivo especificado. Si el archivo no existe o
        /// hay error, restablece los valores por defecto.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                Reset();
                return;
            }
            // Lee los datos encriptados
            try
            {
                string encryptedData = File.ReadAllText(_filePath, Encoding.UTF8);
                SecureConnectionBuilder deserialized = Extensions.Serialization.Utilities
                    .FromJson<SecureConnectionBuilder>(encryptedData);
                Decrypt(deserialized);
                deserialized.MapPropertiesTo(this);
            }
            catch (Exception)
            {
                Reset();
            }
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
        public void Reset()
        {
            ApplicationName = "MyApp";
            InitialCatalog = "master";
            IntegratedSecurity = false;
            Password = string.Empty;
            ProviderName = "System.Data.SqlClient";
            ServerName = "Localhost";
            UserName = string.Empty;
            Encrypt = true;
            TrustServerCertificate = true;
        }

        /// <summary>
        /// Guarda la configuración cifrada en el archivo especificado.
        /// </summary>
        /// <exception cref="DataAccessException">Si ocurre un error durante el guardado.</exception>
        public void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
                throw new InvalidOperationException("La ruta del archivo de configuración no está especificada.");

            string directory = Path.GetDirectoryName(_filePath);
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("No se pudo determinar el directorio del archivo de configuración.");

            try
            {
                Directory.CreateDirectory(directory);

                string jsonEncrypted;
                try
                {
                    jsonEncrypted = EncryptAndSerialize();
                }
                catch (Exception ex)
                {
                    throw new DataAccessException("Error al serializar y encriptar la configuración de conexión.", ex);
                }

                try
                {
                    File.WriteAllText(_filePath, jsonEncrypted, Encoding.UTF8);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new DataAccessException("No se tienen permisos para escribir el archivo de configuración.", ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    throw new DataAccessException("El directorio especificado para el archivo de configuración no existe.", ex);
                }
                catch (IOException ex)
                {
                    throw new DataAccessException("Ocurrió un error de E/S al guardar el archivo de configuración.", ex);
                }
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error inesperado al guardar la configuración de conexión.", ex);
            }
        }

        /// <summary>
        /// Indica si existen los datos mínimos para listar las bases de datos del servidor.
        /// </summary>
        /// <returns>True si se puede listar, false en caso contrario.</returns>
        public bool CanListDatabases() => !string.IsNullOrEmpty(ServerName) &&
            !string.IsNullOrEmpty(ProviderName) &&
            (IntegratedSecurity || (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)));

        /// <summary>
        /// Obtiene un <see cref="DataTable"/> con la lista de bases de datos disponibles en el servidor.
        /// </summary>
        /// <returns>DataTable con columnas: DatabaseName, Owner, CreatedDate.</returns>
        /// <exception cref="DataAccessException">
        /// Si ocurre un error al listar las bases de datos.
        /// </exception>
        public DataTable ListDatabases()
        {
            DataTable dt = new DataTable();
            if (CanListDatabases())
            {
                try
                {
                    using IDaoContext ctx = new DataAccessObjectContext(this);
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
                    throw new DataAccessException("Error al listar las bases de datos.", ex);
                }
            }
            return dt;
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
                using IDaoContext ctx = new DataAccessObjectContext(this);
                ret.PublishedServerName = ctx.GetPublishedServerNameAsync().ConfigureAwait(true).GetAwaiter().GetResult();
                ret.ServerVersion = ctx.Connection.ServerVersion;
            }
            catch (System.Exception ex)
            {
                if (!ex.Data.Contains("Server"))
                    ex.Data.Add("Server", ServerName);
                if (!ex.Data.Contains("Database"))
                    ex.Data.Add("Database", InitialCatalog);
                ret = TestConnectionResult.
                    FailTest(new DataAccessException("No se pudo abrir la conexion a la Base de datos.", ex));
            }
            return ret;
        }

        /// <summary>
        /// Construye la cadena de conexión a partir de las propiedades actuales.
        /// </summary>
        /// <returns>Cadena de conexión resultante.</returns>
        internal virtual string GetCnnString()
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
        /// Descifra las propiedades sensibles de una instancia de <see cref="SecureConnectionBuilder"/>.
        /// </summary>
        /// <param name="cb">Instancia a descifrar.</param>
        private void Decrypt(SecureConnectionBuilder cb)
        {
            foreach (var property in new[]
            {
                    nameof(applicationName),
                    nameof(initialCatalog),
                    nameof(password),
                    nameof(userName),
                    nameof(providerName),
                    nameof(serverName)
                })
            {
                var value = this.GetType()
                    .GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(cb) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var decriptedValue = _encryptionService.Decrypt(value);
                    this.GetType()
                    .GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(cb, decriptedValue);
                }
            }
        }

        /// <summary>
        /// Cifra las propiedades sensibles y serializa la instancia a JSON.
        /// </summary>
        /// <returns>Cadena JSON con los valores cifrados.</returns>
        private string EncryptAndSerialize()
        {
            string res = null;
            try
            {
                foreach (var property in new[]
                {
                        nameof(applicationName),
                        nameof(initialCatalog),
                        nameof(password),
                        nameof(userName),
                        nameof(providerName),
                        nameof(serverName)
                    })
                {
                    var value = this.GetType()
                        .GetField(property,
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(
                        this) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        var encryptedValue = _encryptionService.Encrypt(value);
                        this.GetType()
                        .GetField(property,
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, encryptedValue);
                    }
                }

                res = Extensions.Serialization.Utilities.ToJson(this);
                Decrypt(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un problema al encriptar y serializar: {ex.Message}");
                throw;
            }
            return res;
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
    }
}