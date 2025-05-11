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
        private string applicationName;
        private bool encrypt;
        private string initialCatalog;
        private bool integratedSecurity;
        private string password;
        private string providerName;
        private string serverName;
        private bool trustServerCertificate;
        private string userName;

        #endregion Fields

        #region Constructors

        public SecureConnectionBuilder(Encryption.IEncryptionService encryptionService = null
            , string filePath = "")
        {
            Reset();
            encryptionService ??= Encryption.FactoryEncryptionService.GetAesEncryptionService("@!KUtilities0000");
            _encryptionService = encryptionService;
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(IO.StoreFolder.GetSpecialStoreFolder(IO.SpecialStoreFolder.AllUserPublic),
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
        public string ApplicationName { get => applicationName; set => applicationName = value; }

        [JsonIgnore]
        public string CnnString
        { get => GetCnnString(); }

        [JsonProperty(PropertyName = "E")]
        public bool Encrypt
        { get => encrypt; set => encrypt = value; }

        [JsonIgnore]
        public string Error => "";

        [JsonProperty(PropertyName = "IC")]
        [Required(AllowEmptyStrings = false)]
        public string InitialCatalog
        { get => initialCatalog; set => initialCatalog = value; }

        [JsonProperty(PropertyName = "IS")]
        public bool IntegratedSecurity
        { get => integratedSecurity; set => integratedSecurity = value; }

        [JsonProperty(PropertyName = "P")]
        [RequiredIf("IntegratedSecurity", false)]
        public string Password
        { get => password; set => password = value; }

        [JsonProperty(PropertyName = "PN")]
        [Required(AllowEmptyStrings = false)]
        public string ProviderName
        { get => providerName; set => providerName = value; }

        [JsonProperty(PropertyName = "SN")]
        [Required(AllowEmptyStrings = false)]
        public string ServerName
        { get => serverName; set => serverName = value; }

        [JsonProperty(PropertyName = "TSC")]
        public bool TrustServerCertificate
        { get => trustServerCertificate; set => trustServerCertificate = value; }

        [JsonProperty(PropertyName = "UID")]
        [RequiredIf("IntegratedSecurity", false)]
        public string UserName
        { get => userName; set => userName = value; }

        #endregion Properties

        #region Indexers

        [JsonIgnore]
        public string this[string columnName] => this.GetErrorText(columnName);

        #endregion Indexers

        #region Methods

        public bool IsValid()
            => !DataErrorInfoExt.HasErrors(this);

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
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                string jsonEncrypted = EncryptAndSerialize();
                File.WriteAllText(_filePath, jsonEncrypted, Encoding.UTF8);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CanListDatabases() => !string.IsNullOrEmpty(ServerName)
                && !string.IsNullOrEmpty(ProviderName)
                && (IntegratedSecurity || (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)));
        public DataTable ListDatabases()
        {
            DataTable dt = new DataTable();
            if (CanListDatabases())
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
                try
                {
                    using (DbConnection connection = factory.CreateConnection())
                    {
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
                            newRow["DatabaseName"] = row.Table.Columns.Contains("database_name") ? row["database_name"] : row["DATABASE_NAME"];
                            newRow["Owner"] = row.Table.Columns.Contains("owner") ? row["owner"] : DBNull.Value;
                            newRow["CreatedDate"] = row.Table.Columns.Contains("created") ? row["created"] : DBNull.Value;
                            dt.Rows.Add(newRow);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new DataDbException("Error al listar las bases de datos.", "ListDatabases", ex);
                }
            }
            return dt;
        }
        public TestConnectionResult TestConnection()
        {
            TestConnectionResult ret = TestConnectionResult.SucessTest;
            DbProviderFactory factory;
            factory = DbProviderFactories.GetFactory(ProviderName);
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = CnnString;
                
                try
                {
                    connection.Open();
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT PUBLISHINGSERVERNAME() as Servername";
                        command.CommandType = System.Data.CommandType.Text;
                        ret.PublishedServerName=(string)command.ExecuteScalar();
                        ret.ServerVersion = connection.ServerVersion;
                    }
                }
                catch (System.Exception ex)
                {
                    if (!ex.Data.Contains("Server")) ex.Data.Add("Server", ServerName);
                    if (!ex.Data.Contains("Database")) ex.Data.Add("Database", initialCatalog);
                    ret = TestConnectionResult.
                        FailTest(new DataDbException("No se pudo abrir la conexion a la Base de datos.", "Prueba de conexión", ex));
                }
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
            foreach (var property in new[] { nameof(applicationName), nameof(initialCatalog), nameof(password), nameof(userName), nameof(providerName), nameof(serverName) })
            {
                var value = GetType().GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(cb) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var decriptedValue = _encryptionService.Decrypt(value);
                    GetType().GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(cb, decriptedValue);
                }
            }
        }

        private string EncryptAndSerialize()
        {
            string res = null;
            try
            {
                foreach (var property in new[] { nameof(applicationName), nameof(initialCatalog), nameof(password), nameof(userName), nameof(providerName), nameof(serverName) })
                {
                    var value = GetType().GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(this) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        var encryptedValue = _encryptionService.Encrypt(value);
                        GetType().GetField(property, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, encryptedValue);
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
        {
            return DbProviderFactories.GetProviderInvariantNames();
        }
#endif
        #endregion Methods
    }
}