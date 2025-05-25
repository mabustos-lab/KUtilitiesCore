using KUtilitiesCore.Data;
using KUtilitiesCore.Data.ValidationAttributes;
using KUtilitiesCore.DataAccess.Helpers;
using KUtilitiesCore.DataAccess.Utils;
using KUtilitiesCore.Extensions;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace KUtilitiesCore.DataAccess
{
    public class SecureConnectionBuilder : IConnectionBuilder, INotifyPropertyChanged
    {
        #region Fields
        private readonly Encryption.IEncryptionService _encryptionService;
        private readonly string _filePath;
        #endregion Fields

        #region Constructors
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
        #endregion Constructors

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Properties
        [JsonProperty(PropertyName = "AN")]
        public string ApplicationName { get; set; }

        [JsonIgnore]
        public string CnnString { get => GetCnnString(); }

        [JsonProperty(PropertyName = "E")]
        public bool Encrypt { get; set; }

        [JsonIgnore]
        public string Error => string.Empty;

        [JsonProperty(PropertyName = "IC")]
        [Required(AllowEmptyStrings = false)]
        public string InitialCatalog { get; set; }

        [JsonProperty(PropertyName = "IS")]
        public bool IntegratedSecurity { get; set; }

        [JsonProperty(PropertyName = "P")]
        [RequiredIfAttribute("IntegratedSecurity", false)]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "PN")]
        [Required(AllowEmptyStrings = false)]
        public string ProviderName { get; set; }

        [JsonProperty(PropertyName = "SN")]
        [Required(AllowEmptyStrings = false)]
        public string ServerName { get; set; }

        [JsonProperty(PropertyName = "TSC")]
        public bool TrustServerCertificate { get; set; }

        [JsonProperty(PropertyName = "UID")]
        [RequiredIfAttribute("IntegratedSecurity", false)]
        public string UserName { get; set; }
        #endregion Properties

        #region Indexers
        [JsonIgnore]
        public string this[string columnName] => this.GetErrorText(columnName);
        #endregion Indexers

        #region Methods
        public bool IsValid() => !DataErrorInfoExt.HasErrors(this);

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

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

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

        public bool CanListDatabases() => !string.IsNullOrEmpty(ServerName) &&
            !string.IsNullOrEmpty(ProviderName) &&
            (IntegratedSecurity || (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)));

        public DataTable ListDatabases()
        {
            DataTable dt = new DataTable();
            if (CanListDatabases())
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
                try
                {
                    using DbConnection connection = factory.CreateConnection();
                    connection.ConnectionString = CnnString;
                    connection.Open();

                    // Obtener los catálogos (bases de datos)
                    DataTable schemaTable = connection.GetSchema("Databases");

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

        public virtual TestConnectionResult TestConnection()
        {
            TestConnectionResult ret = TestConnectionResult.SucessTest;
            DbProviderFactory factory;
            factory = DbProviderFactories.GetFactory(ProviderName);
            using DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = CnnString;
            try
            {
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT PUBLISHINGSERVERNAME() as Servername";
                    command.CommandType = System.Data.CommandType.Text;
                    ret.PublishedServerName = (string)command.ExecuteScalar();
                    ret.ServerVersion = connection.ServerVersion;
                }
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
                // Añadir otros parámetros si son necesarios (e.g., Connection Timeout, Pooling)
                // sb.Append("Connection Timeout=30;");
                res = dbcsb.ToString();
            }
            catch (Exception)
            {
                throw;
            }

            return res;
        }

        private void Decrypt(SecureConnectionBuilder cb)
        {
            foreach (var property in new[]
            {
                nameof(ApplicationName),
                nameof(InitialCatalog),
                nameof(Password),
                nameof(UserName),
                nameof(ProviderName),
                nameof(ServerName)
            })
            {
                var value = GetType()
                    .GetField(
                        property,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(
                    cb) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var decriptedValue = _encryptionService.Decrypt(value);
                    GetType()
                    .GetField(
                        property,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(
                    cb,
                    decriptedValue);
                }
            }
        }

        private string EncryptAndSerialize()
        {
            string res = null;
            try
            {
                foreach (var property in new[]
                {
                    nameof(ApplicationName),
                    nameof(InitialCatalog),
                    nameof(Password),
                    nameof(UserName),
                    nameof(ProviderName),
                    nameof(ServerName)
                })
                {
                    var value = GetType()
                        .GetField(
                            property,
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(
                        this) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        var encryptedValue = _encryptionService.Encrypt(value);
                        GetType()
                        .GetField(
                            property,
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(
                        this,
                        encryptedValue);
                    }
                }

                res = Extensions.Serialization.Utilities.ToJson(this);
                Decrypt(this);
            }
            catch (Exception)
            {
                throw;
            }
            return res;
        }
#if NET8_0_OR_GREATER

        public static IEnumerable<string> GetProviderInvariantNames()
        { return DbProviderFactories.GetProviderInvariantNames(); }
#endif
        #endregion Methods
    }
}