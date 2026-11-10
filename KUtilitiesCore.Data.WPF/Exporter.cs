using KUtilitiesCore.Data.DataExporter;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KUtilitiesCore.Data.WPF
{
    /// <summary>
    /// Punto de entrada de exportación de datos en entornos WPF: fachada del diálogo
    /// nativo "Guardar como" (<see cref="Microsoft.Win32.SaveFileDialog"/>) sobre la
    /// preparación compartida (<see cref="ExportDialogPreparation"/>), que valida el
    /// origen, sugiere el nombre, aporta el filtro y resuelve el formato por extensión
    /// antes de delegar en los exportadores de <c>KUtilitiesCore.Data</c>. Centraliza
    /// aquí la interacción con el usuario para que las librerías de exportación
    /// permanezcan independientes de la UI.
    /// </summary>
    /// <remarks>
    /// Paridad contractual con <c>KUtilitiesCore.Data.Win.Exporter</c>: ambas fachadas
    /// comparten validación de origen, nombre por defecto, filtro y resolución de
    /// formato a través de <see cref="ExportDialogPreparation"/>, diferenciándose solo
    /// en el diálogo nativo de cada plataforma. No depende de Windows Forms.
    /// </remarks>
    public static class Exporter
    {
        /// <summary>
        /// Exporta un DataTable a los archivos soportados (*.csv;*.tsv;*.psv;*.xlsx;*.xls)
        /// mostrando un diálogo nativo de WPF para seleccionar el destino.
        /// </summary>
        /// <param name="dt">Tabla de datos a exportar. Debe contener al menos una fila.</param>
        /// <param name="titleDialog">Título del diálogo de guardado.</param>
        /// <param name="defaulFilename">Nombre de archivo sugerido; si está vacío se genera
        /// uno con sello temporal a través de <see cref="ExportDialogPreparation.GetDefaultFileName"/>.</param>
        /// <returns><see langword="true"/> si la exportación se completó; <see langword="false"/>
        /// si el origen no es válido, el usuario canceló el diálogo o la exportación falló.</returns>
        /// <example>
        /// <code>
        /// var tabla = ObtenerTabla();
        /// if (tabla.ShowExportDataDialog())
        /// {
        ///     // El archivo fue exportado y, si el usuario lo pidió, abierto.
        /// }
        /// </code>
        /// </example>
        public static bool ShowExportDataDialog(
            this DataTable dt,
            string titleDialog = "Guardar exportación de datos.",
            string defaulFilename = "")
        {
            if (!ExportDialogPreparation.ValidateSource(dt))
            {
                MessageBox.Show("No hay datos para exportar.", "Exportación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(defaulFilename))
                defaulFilename = ExportDialogPreparation.GetDefaultFileName();

            Microsoft.Win32.SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = ExportDialogPreparation.FileDialogFilter;
            saveFileDialog.Title = titleDialog;
            saveFileDialog.FileName = defaulFilename;
            saveFileDialog.DefaultExt = ".xlsx";
            saveFileDialog.AddExtension = true;

            if (saveFileDialog.ShowDialog() != true)
                return false;

            string selectedFile = saveFileDialog.FileName;
            string fileExtension = Path.GetExtension(selectedFile).ToLowerInvariant();

            try
            {
                bool abrirAlFinalizar = MessageBox.Show(
                    "¿Desea abrir el archivo después de exportar?",
                    "Exportación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes;

                Mouse.OverrideCursor = Cursors.Wait;

                switch (ExportDialogPreparation.ResolveFormat(selectedFile))
                {
                    case ExportFormat.Excel:
                        dt.ToExcel(selectedFile, abrirAlFinalizar);
                        break;

                    case ExportFormat.Text:
                        string separator = GetSeparator(selectedFile);
                        dt.ToCsv(selectedFile, abrirAlFinalizar, separator);
                        break;

                    default:
                        MessageBox.Show($"La extensión '{fileExtension}' no está soportada.",
                            "Formato no válido",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                }

                if (abrirAlFinalizar)
                {
                    MessageBox.Show("Archivo exportado exitosamente y abierto en la aplicación predeterminada.",
                        "Exportación completada",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Archivo exportado exitosamente a:\n{selectedFile}",
                        "Exportación completada",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                return true;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show($"Formato de archivo no soportado: {ex.Message}",
                    "Error de formato",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No tiene permisos para escribir en la ubicación seleccionada.",
                    "Error de permisos",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error de E/S: {ex.Message}\n\nVerifique que el archivo no esté en uso por otra aplicación.",
                    "Error de acceso al archivo",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en exportación: {ex}");

                MessageBox.Show($"Error al exportar el archivo: {ex.Message}",
                    "Error de exportación",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Devuelve el separador según la extensión del archivo.
        /// </summary>
        /// <param name="fileName">Nombre o ruta completa del archivo.</param>
        /// <returns>Separador como string (",", "\t", "|").</returns>
        /// <remarks>Fachada sobre <see cref="ExportDialogPreparation.GetSeparator"/>: la
        /// correspondencia extensión→separador es conocimiento de exportación compartido
        /// entre plataformas.</remarks>
        public static string GetSeparator(string fileName)
            => ExportDialogPreparation.GetSeparator(fileName);
    }
}
