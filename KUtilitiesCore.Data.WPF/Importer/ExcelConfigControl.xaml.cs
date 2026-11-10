using System.Windows.Controls;

namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Panel de configuración de análisis Excel del asistente de importación: hoja a leer,
/// encabezados y rango de filas (fila final en cero significa "todas", hasta el final).
/// </summary>
/// <remarks>
/// Vista fina sobre <see cref="ExcelConfigViewModel"/>: no posee estado propio, solo enlaza
/// las opciones del ViewModel (el DataContext lo hereda del ContentControl que lo hospeda en
/// <see cref="ImportWizardWindow"/>). Paridad con el <c>ExcelConfigControl</c> de WinForms,
/// incluida la semántica de la fila final en cero.
/// </remarks>
public partial class ExcelConfigControl : UserControl
{
    /// <summary>Inicializa la vista y carga sus enlaces XAML.</summary>
    public ExcelConfigControl() => InitializeComponent();
}
