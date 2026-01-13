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
            // Validación del DataTable
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportación",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Configurar nombre de archivo por defecto
            if (string.IsNullOrEmpty(defaulFilename))
                defaulFilename = $"Exportacion_{DateTime.Now:yyyyMMdd_HHmmss}";

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Configurar el diálogo
                saveFileDialog.Filter = "Todos los archivos soportados|*.csv;*.tsv;*.psv;*.xlsx;*.xls|" +
                                       "Archivos de Texto (*.csv, *.tsv, *.psv)|*.csv;*.tsv;*.psv|" +
                                       "Archivos de Excel (*.xlsx)|*.xlsx|" +
                                       "Archivos de Excel 97-2003 (*.xls)|*.xls";
                saveFileDialog.Title = titleDialog;
                saveFileDialog.FileName = defaulFilename;
                saveFileDialog.DefaultExt = ".csv"; // Extensión por defecto
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

                    // Procesar según la extensión
                    Cursor.Current = Cursors.WaitCursor;

                    switch (fileExtension)
                    {
                        case ".xlsx":
                        case ".xls":
                            dt.ToExcel(selectedFile, abrirAlFinalizar);
                            break;

                        case ".csv":
                        case ".tsv":
                        case ".psv":
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
        public static string GetSeparator(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("El nombre del archivo no puede estar vacío.", nameof(fileName));

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".csv" => ",",
                ".tsv" => "\t",
                ".psv" => "|",
                _ => throw new NotSupportedException($"La extensión '{extension}' no está soportada.")
            };
        }

    }
}
