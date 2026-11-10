using KUtilitiesCore.Data.DataImporter.Interfaces;

namespace KUtilitiesCore.Data.Wizard.Configuration;

/// <summary>
/// Contrato compartido del panel de configuración de análisis (CSV/Excel) del asistente de
/// importación.
/// </summary>
/// <remarks>
/// <para>
/// Vive en un namespace propio para coexistir sin ambigüedad con la interfaz homónima de
/// <c>KUtilitiesCore.Data.Win.Importer</c>, que hereda de este contrato: las implementaciones
/// WinForms existentes (<c>CsvConfigControl</c>, <c>ExcelConfigControl</c>) siguen compilando sin
/// cambios de firma y el orquestador solo conoce esta versión compartida.
/// </para>
/// <para>
/// El host es responsable de inicializar el panel con <see cref="Initialize"/> tras seleccionar
/// un archivo y de suscribir <see cref="OptionsChanged"/> después de la inicialización, para
/// ignorar los eventos que el propio panel emite al restaurar su estado.
/// </para>
/// </remarks>
public interface IImportConfigControl
{
    /// <summary>
    /// Notifica que el usuario modificó las opciones de análisis después de la inicialización.
    /// </summary>
    /// <remarks>
    /// Debe dispararse solo por interacción real del usuario, nunca durante
    /// <see cref="Initialize"/>, para que el orquestador no invalide datos al montar el panel.
    /// </remarks>
    event EventHandler? OptionsChanged;

    /// <summary>
    /// Restaura el estado del panel para el archivo seleccionado.
    /// </summary>
    /// <param name="fileName">Ruta del archivo cuyas opciones se van a configurar.</param>
    void Initialize(string fileName);

    /// <summary>
    /// Devuelve las opciones de análisis actuales del panel.
    /// </summary>
    /// <returns>
    /// Opciones listas para entregarse a la fábrica de lectores correspondiente
    /// (<c>TextFileParsingOptions</c> para texto o <c>ExcelParsingOptions</c> para Excel).
    /// </returns>
    IParsingOptions GetParsingOptions();
}
