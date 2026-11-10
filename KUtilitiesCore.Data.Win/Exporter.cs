using KUtilitiesCore.Data.DataExporter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Win
{
    /// <summary>
    /// Punto de entrada de exportación de datos en entornos WinForms: fachada del diálogo
    /// nativo "Guardar como" sobre la preparación compartida
    /// (<see cref="ExportDialogPreparation"/>), que valida el origen, sugiere el nombre,
    /// aporta el filtro y resuelve el formato por extensión antes de delegar en los
    /// exportadores de <c>KUtilitiesCore.Data</c>. Centraliza aquí la interacción con el
    /// usuario para que las librerías de exportación permanezcan independientes de la UI.
    /// </summary>
    public static class Exporter
    {
        /// <summary>
        /// Exporta un DataTable a los archivos soportado (*.csv;*.tsv;*.psv;*.xlsx;*.xls)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="titleDialog"></param>
        /// <param name="defaulFilename"></param>
        /// <returns></returns>
        public static bool ShowExportDataDialog(
            this DataTable dt,
            string titleDialog = "Guardar exportación de datos.",
            string defaulFilename = "")
        {
            // Validación del origen delegada en la preparación compartida: una tabla sin
            // filas se rechaza ANTES de abrir el diálogo (paridad con la fachada WPF).
            if (!ExportDialogPreparation.ValidateSource(dt))
            {
                MessageBox.Show("No hay datos para exportar.", "Exportación",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Nombre de archivo por defecto con sello temporal, delegado en la capa compartida.
            if (string.IsNullOrEmpty(defaulFilename))
                defaulFilename = ExportDialogPreparation.GetDefaultFileName();

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Configurar el diálogo (filtro compartido para garantizar paridad de opciones).
                saveFileDialog.Filter = ExportDialogPreparation.FileDialogFilter;
                saveFileDialog.Title = titleDialog;
                saveFileDialog.FileName = defaulFilename;
                saveFileDialog.DefaultExt = ".xlsx"; // Extensión por defecto
                saveFileDialog.AddExtension = true; // Añadir extensión automáticamente

                // Mostrar diálogo y validar selección
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return false;

                string selectedFile = saveFileDialog.FileName;
                string fileExtension = Path.GetExtension(selectedFile).ToLowerInvariant();

                try
                {
                    // Preguntar si desea abrir el archivo antes de procesar
                    bool abrirAlFinalizar = MessageBox.Show(
                        "¿Desea abrir el archivo después de exportar?",
                        "Exportación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes;

                    // Procesar según el formato resuelto por la preparación compartida
                    Cursor.Current = Cursors.WaitCursor;

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
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Error);
                            return false;
                    }

                    // Confirmación de éxito
                    if (abrirAlFinalizar)
                    {
                        MessageBox.Show($"Archivo exportado exitosamente y abierto en la aplicación predeterminada.",
                                      "Exportación completada",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Archivo exportado exitosamente a:\n{selectedFile}",
                                      "Exportación completada",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);
                    }

                    return true;
                }
                catch (NotSupportedException ex)
                {
                    MessageBox.Show($"Formato de archivo no soportado: {ex.Message}",
                                  "Error de formato",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("No tiene permisos para escribir en la ubicación seleccionada.",
                                  "Error de permisos",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return false;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error de E/S: {ex.Message}\n\nVerifique que el archivo no esté en uso por otra aplicación.",
                                  "Error de acceso al archivo",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    // Log para depuración (opcional)
                    System.Diagnostics.Debug.WriteLine($"Error en exportación: {ex}");

                    MessageBox.Show($"Error al exportar el archivo: {ex.Message}",
                                  "Error de exportación",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Devuelve el separador según la extensión del archivo.
        /// </summary>
        /// <param name="fileName">Nombre o ruta completa del archivo.</param>
        /// <returns>Separador como string (",", "\t", "|").</returns>
        /// <remarks>Fachada sobre <see cref="ExportDialogPreparation.GetSeparator"/>: la correspondencia extensión→separador es conocimiento de exportación compartido entre plataformas.</remarks>
        public static string GetSeparator(string fileName)
            => ExportDialogPreparation.GetSeparator(fileName);

    }
}
