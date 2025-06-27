using KUtilitiesCore.GitHubUpdater.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using KUtilitiesCore.Encryption;
using KUtilitiesCore.Encryption.Extensions;
using System.Diagnostics;
using KUtilitiesCore.GitHubUpdater.Helpers;


#if NET8_0_OR_GREATER
using System.Text.Json.Serialization;
#else

using Newtonsoft.Json;

#endif

namespace KUtilitiesCore.GitHubUpdater
{
    /// <summary>
    /// Implementa la información y lógica necesaria para gestionar la actualización de una
    /// aplicación desde GitHub. Permite distinguir entre diferentes canales de actualización (por
    /// ejemplo, QA y Producción), integrando la gestión de versiones a través de Releases de GitHub
    /// y facilitando la extensión a módulos adicionales como el reporte de errores.
    /// </summary>
    public sealed class AppUpdateInfo : IAppUpdateInfo
    {
        #region Fields

        /// <summary>
        /// Servicio de cifrado utilizado para proteger los datos sensibles.
        /// </summary>
        [JsonIgnore]
        private readonly IEncryptionService _encryptionService;

        /// <summary>
        /// Ruta del archivo donde se almacena la configuración.
        /// </summary>
        [JsonIgnore]
        private readonly string _filePath;

        /// <summary>
        /// Información del repositorio de GitHub asociada a la aplicación.
        /// </summary>
        private readonly GitHubRepositoryInfo _gitHub;

        /// <summary>
        /// Token seguro almacenado en memoria para operaciones temporales.
        /// </summary>
        [JsonIgnore]
        private SecureString secureToken;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AppUpdateInfo"/>.
        /// </summary>
        /// <param name="encryptionService">
        /// Servicio de cifrado a utilizar. Si es nulo, se utiliza el servicio AES por defecto.
        /// </param>
        /// <param name="filePath">
        /// Ruta del archivo de configuración. Si es vacío, se utiliza la ruta predeterminada.
        /// </param>
        public AppUpdateInfo(Encryption.IEncryptionService? encryptionService = null,
            string filePath = "")
        {
            encryptionService ??= FactoryEncryptionService.GetAesEncryptionService("@!KUtilities0000");
            _encryptionService = encryptionService;
            UpdateChannel = string.Empty;
            AppVersion = string.Empty;

            if (string.IsNullOrEmpty(filePath))
                filePath = Path.Combine(
                    Path.GetTempPath(),
                    "KUtilitiesCore",
                    "GitHubRepositoryInfo.json");
            _filePath = filePath;

            secureToken = new SecureString();
            _gitHub = new GitHubRepositoryInfo();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Versión actual de la aplicación. Utilizada para comparar con la última release
        /// disponible en GitHub.
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Información del repositorio de GitHub donde se gestionan las releases y otros módulos
        /// como el reporte de errores.
        /// </summary>
        public IGitHubRepositoryInfo GitHub => _gitHub;

        /// <summary>
        /// Canal de actualización actual (por ejemplo, "QA", "Production"). Permite seleccionar el
        /// flujo de releases adecuado según el entorno.
        /// </summary>
        public string UpdateChannel { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Obtiene el token desencriptado (solo cuando se necesita).
        /// </summary>
        /// <returns>Token de acceso personal en texto plano, o cadena vacía si no está disponible.</returns>
        public string GetDecryptedToken()
        {
            if (secureToken.Length > 0)
                return secureToken.ToPlainText();

            if (!string.IsNullOrEmpty(GitHub.EncryptedToken))
            {
                secureToken = GitHub.GetSecuredToken(_encryptionService);
                return _encryptionService.Decrypt(GitHub.EncryptedToken);
            }

            return string.Empty;
        }

        /// <summary>
        /// Guarda la configuración de actualización de la aplicación.
        /// </summary>
        public void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
                throw new InvalidOperationException("La ruta del archivo de configuración no está especificada.");

            string directory = Path.GetDirectoryName(_filePath)!;
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("No se pudo determinar el directorio del archivo de configuración.");
            string res = string.Empty;
            try
            {
                Directory.CreateDirectory(directory);
                res = Helpers.Utilities.ToJson(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un problema al encriptar y serializar: {ex.Message}");
                throw;
            }
            try
            {
                File.WriteAllText(_filePath, res, Encoding.UTF8);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"No se tienen permisos para escribir el archivo de configuración: {ex.Message}");
                throw;
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.WriteLine($"El directorio especificado para el archivo de configuración no existe: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Ocurrió un error de E/S al guardar el archivo de configuración: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Asigna el token en texto plano y lo encripta para almacenamiento seguro.
        /// </summary>
        /// <param name="token">Token de acceso personal en texto plano.</param>
        /// <exception cref="ArgumentNullException">Si el token es nulo o vacío.</exception>
        public void SetPlaintextToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            _gitHub.EncryptedToken = _encryptionService.Encrypt(token);
            secureToken = token.ToSecureString();
        }
        /// <summary>
        /// Restaura la infomación establecida desde un objeto serializado en formato JSON
        /// </summary>
        /// <param name="json"></param>
        public void LoadJson(string json)
        {
            try
            {
                AppUpdateInfo deserialized= Utilities.FromJson<AppUpdateInfo>(json);
                AppVersion=deserialized.AppVersion;
                UpdateChannel = deserialized.UpdateChannel;
                _gitHub.EncryptedToken= deserialized.GitHub.EncryptedToken;
                _gitHub.Repository=deserialized.GitHub.Repository;
                _gitHub.IssueLabels=deserialized.GitHub.IssueLabels;
                _gitHub.Owner=deserialized.GitHub.Owner;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un problema al deserializar el objeto: {ex.Message}");
                throw;
            }
        }

        #endregion Methods
    }
}