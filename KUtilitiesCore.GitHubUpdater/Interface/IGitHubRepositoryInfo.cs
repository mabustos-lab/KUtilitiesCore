using System;
using System.Linq;

namespace KUtilitiesCore.GitHubUpdater.Interface
{
    /// <summary>
    /// Contrato que representa la información esencial de un repositorio de GitHub.
    /// Incluye los datos necesarios para acceder a releases, gestionar issues y
    /// soportar módulos adicionales como el reporte de errores.
    /// </summary>
    public interface IGitHubRepositoryInfo
    {
        /// <summary>
        /// Propietario del repositorio de GitHub (usuario u organización).
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Nombre del repositorio de GitHub.
        /// </summary>
        string Repository { get; set; }

        /// <summary>
        /// Token de acceso personal para autenticación con la API de GitHub.
        /// Requerido para operaciones autenticadas como la creación de issues.
        /// </summary>
        string EncryptedToken { get; }

        /// <summary>
        /// Etiquetas que se asignarán automáticamente a los issues creados (por ejemplo, para reporte de errores).
        /// </summary>
        string[] IssueLabels { get; set; }
    }
}
