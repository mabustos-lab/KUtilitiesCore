using KUtilitiesCore.Dal.Exceptions;
using KUtilitiesCore.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace KUtilitiesCore.Dal.ConnectionBuilder
{
    /// <summary>
    /// Proporciona una implementación segura de <see cref="IConnectionBuilder"/> que permite
    /// construir, almacenar y recuperar cadenas de conexión a bases de datos de forma cifrada.
    /// Incluye soporte para notificación de cambios de propiedades y validación de datos.
    /// </summary>
    public class SecureConnectionBuilder : ConnectionBuilderBase, ISecureConnectionBuilder
    {
        #region Fields

        /// <summary>
        /// Servicio de cifrado utilizado para proteger los datos sensibles.
        /// </summary>
        private readonly Encryption.IEncryptionService _encryptionService;

        /// <summary>
        /// Ruta del archivo donde se almacena la configuración cifrada.
        /// </summary>
        private readonly string _filePath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor vacio para la Deserialización de la clase
        /// </summary>
        public SecureConnectionBuilder() : this(null, string.Empty)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="SecureConnectionBuilder"/>.
        /// </summary>
        /// <param name="encryptionService">
        /// Servicio de cifrado a utilizar. Si es nulo, se utiliza el servicio AES por defecto.
        /// </param>
        /// <param name="filePath">
        /// Ruta del archivo de configuración. Si es vacío, se utiliza la ruta predeterminada.
        /// </param>
        public SecureConnectionBuilder(Encryption.IEncryptionService encryptionService, string filePath = "")
        {
            Reset();
            encryptionService ??= Encryption.FactoryEncryptionService.GetAesEncryptionService("@!KUtilities0000");
            _encryptionService = encryptionService;
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(
                    IO.StoreFolder.GetSpecialStoreFolder(IO.SpecialStoreFolder.AllUserPublic),
                    Assembly.GetEntryAssembly()?.GetName().Name ?? Process.GetCurrentProcess().ProcessName,
                    "SecureConnection.enc");
            _filePath = filePath;
        }

        #endregion Constructors

        #region Methods

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
                    Debug.WriteLine(ex.Message);
                    throw;
                }

                try
                {
                    File.WriteAllText(_filePath, jsonEncrypted, Encoding.UTF8);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
                catch (DirectoryNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
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
                    .GetField(
                        property,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(
                    cb) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    var decriptedValue = _encryptionService.Decrypt(value);
                    this.GetType()
                    .GetField(
                        property,
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(
                    cb,
                    decriptedValue);
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
                        .GetField(
                            property,
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(
                        this) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        var encryptedValue = _encryptionService.Encrypt(value);
                        this.GetType()
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un problema al encriptar y serializar: {ex.Message}");
                throw;
            }
            return res;
        }

        #endregion Methods
    }
}