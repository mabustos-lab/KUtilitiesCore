using KUtilitiesCore.Data.DataExporter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Win
{
    public static class Exporter
    {
        /// <summary>
        /// Exporta un DataTable a un archivo Excel (.xlsx).
        /// </summary>
        public static void ShowDialogToExcel(this DataTable dt, bool openFile = true)
        {
            // Configurar el diálogo para guardar
            using(SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Archivos de Excel (*.xlsx)|*.xlsx|Archivos de Excel 97-2003 (*.xls)|*.xls";
                saveFileDialog.Title = "Guardar exportación de datos";
                saveFileDialog.FileName = $"Exportacion_{DateTime.Now:yyyyMMdd_HHmmss}";
                if(saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bool abrirAlFinalizar = false;
                    abrirAlFinalizar = MessageBox.Show(
                            "¿Desea abrir el archivo ahora?",
                            "Exportación",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) ==
                        DialogResult.Yes;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        dt.ToExcel(saveFileDialog.FileName, abrirAlFinalizar);
                        Cursor.Current = Cursors.Default;
                    } catch(Exception ex)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show($"Ocurrio un problema al generar el archivo. {ex.Message}");
                    }
                }
            }
        }

            /// <summary>
            /// Exporta un DataTable a un archivo Excel (.CSV).
            /// </summary>
        public static void ShowDialogToCSV(this DataTable dt, bool openFile = true)
        {
            // Configurar el diálogo para guardar
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Texto separado por comas (*.csv)|*.csv|Texto separado por Tabs (*.tsv)|*.tsv|Texto separado por Pipes (*.psv)|*.psv";
                saveFileDialog.Title = "Guardar exportación de datos";
                saveFileDialog.FileName = $"Exportacion_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bool abrirAlFinalizar = false;
                    abrirAlFinalizar = MessageBox.Show(
                            "¿Desea abrir el archivo ahora?",
                            "Exportación",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) ==
                        DialogResult.Yes;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        dt.ToCsv(saveFileDialog.FileName, abrirAlFinalizar, GetSeparator(saveFileDialog.FileName));
                        Cursor.Current = Cursors.Default;
                    }
                    catch (Exception ex)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show($"Ocurrio un problema al generar el archivo. {ex.Message}");
                    }
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
