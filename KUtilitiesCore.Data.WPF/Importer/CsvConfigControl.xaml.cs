using System.Windows.Controls;

namespace KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Panel de configuración de análisis CSV del asistente de importación: separador,
/// encabezados, recorte de valores y líneas vacías.
/// </summary>
/// <remarks>
/// Vista fina sobre <see cref="CsvConfigViewModel"/>: no posee estado propio, solo enlaza las
/// opciones del ViewModel (el DataContext lo hereda del ContentControl que lo hospeda en
/// <see cref="ImportWizardWindow"/>). Paridad con el <c>CsvConfigControl</c> de WinForms;
/// la selección de codificación quedó diferida deliberadamente (ver las notas de
/// <see cref="CsvConfigViewModel"/>).
/// </remarks>
public partial class CsvConfigControl : UserControl
{
    /// <summary>Inicializa la vista y carga sus enlaces XAML.</summary>
    public CsvConfigControl() => InitializeComponent();
}
