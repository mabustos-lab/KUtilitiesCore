using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Define la información necesaria para gestionar la actualización de una aplicación desde
    /// GitHub. Permite distinguir entre diferentes canales de actualización (por ejemplo, QA y
    /// Producción), integrando la gestión de versiones a través de Releases de GitHub y facilitando
    /// la extensión a módulos adicionales como el reporte de errores.
    /// </summary>
    /// <remarks>
    /// Los Tag usados en GitHub serian por ejemplo:
    /// <code>v1.2.3-qa</code>
    /// <code>v1.2.3-prod</code>
    /// </remarks>
    public interface IAppUpdateInfo
    {

        /// <summary>
        /// Versión actual de la aplicación. Utilizada para comparar con la última release
        /// disponible en GitHub.
        /// </summary>
        string AppVersion { get; set; }

        /// <summary>
        /// Información del repositorio de GitHub donde se gestionan las releases y otros módulos
        /// como el reporte de errores.
        /// </summary>
        IGitHubRepositoryInfo GitHub { get; }

        /// <summary>
        /// Canal de actualización actual (por ejemplo, "QA", "Production"). Permite seleccionar el
        /// flujo de releases adecuado según el entorno.
        /// </summary>
        string UpdateChannel { get; set; }

        /// <summary>
        /// Obtiene el token desencriptado (solo cuando se necesita).
        /// </summary>
        /// <returns>Token de acceso personal en texto plano, o cadena vacía si no está disponible.</returns>
        string GetDecryptedToken();

        /// <summary>
        /// Restaura la infomación establecida desde un objeto serializado en formato JSON
        /// </summary>
        void LoadJson(string json);

        /// <summary>
        /// Guarda la configuración de actualización de la aplicación.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Asigna el token en texto plano y lo encripta para almacenamiento seguro.
        /// </summary>
        /// <param name="token">Token de acceso personal en texto plano.</param>
        /// <exception cref="ArgumentNullException">Si el token es nulo o vacío.</exception>
        void SetPlaintextToken(string token);

    }
}