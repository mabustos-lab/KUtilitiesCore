using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Define la información necesaria para gestionar la actualización de una aplicación desde GitHub.
    /// Permite distinguir entre diferentes canales de actualización (por ejemplo, QA y Producción),
    /// integrando la gestión de versiones a través de Releases de GitHub y facilitando la extensión
    /// a módulos adicionales como el reporte de errores.
    /// </summary>
    /// <remarks>Los Tag usados en GitHub serian por ejemplo:<code>v1.2.3-qa</code><code>v1.2.3-prod</code></remarks>
    public interface IAppUpdateInfo
    {
        /// <summary>
        /// Canal de actualización actual (por ejemplo, "QA", "Production").
        /// Permite seleccionar el flujo de releases adecuado según el entorno.
        /// </summary>
        string UpdateChannel { get; set; }

        /// <summary>
        /// Versión actual de la aplicación.
        /// Utilizada para comparar con la última release disponible en GitHub.
        /// </summary>
        string AppVersion { get; set; }

        /// <summary>
        /// Información del repositorio de GitHub donde se gestionan las releases y otros módulos
        /// como el reporte de errores.
        /// </summary>
        IGitHubRepositoryInfo GitHub { get; }
    }
}
