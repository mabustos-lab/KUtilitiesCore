using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KUtilitiesCore.Data.Win.Importer
{
    /// <summary>
    /// Control de configuración para archivos Excel: muestra las hojas del libro,
    /// permite indicar si la primera fila es cabecera y acota el rango de filas a
    /// leer. El asistente de importación lo hospeda cuando el archivo
    /// seleccionado es .xlsx o .xls.
    /// </summary>
    public partial class ExcelConfigControl : UserControl, IImportConfigControl
    {
        /// <summary>
        /// Inicializa el control y enlaza los cambios de sus elementos con
        /// <see cref="OptionsChanged"/>.
        /// </summary>
        public ExcelConfigControl()
        {
            InitializeComponent();
            cboSheet.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            chkHasHeader.CheckedChanged += (a, b) => OnOptionsChanged();
            numStartRow.ValueChanged += (a, b) => OnOptionsChanged();
            numEndRow.ValueChanged += (a, b) => OnOptionsChanged();
        }
        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <inheritdoc/>
        public event EventHandler? OptionsChanged;

        /// <inheritdoc/>
        public IParsingOptions GetParsingOptions()
        {
            return new ExcelParsingOptions
            {
                // SelectedText siempre esta vacio en un ComboBox DropDownList;
                // el elemento marcado vive en SelectedItem. Sin esto el lector
                // recibia una cadena vacia y no la hoja elegida por el usuario.
                SheetName = cboSheet.SelectedItem?.ToString(),
                HasHeader = chkHasHeader.Checked,
                StartRow = (int)numStartRow.Value,
                // El contrato de ExcelParsingOptions define EndRow = null como
                // "leer hasta el final"; el 0 de la UI es solo representacion.
                EndRow = numEndRow.Value > 0 ? (int)numEndRow.Value : null
            };
        }

        /// <summary>
        /// Agrega una hoja al combo para pruebas automatizadas; no debe usarse
        /// en codigo de produccion porque el combo se llena desde <see cref="Initialize"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void AddSheetForTesting(string sheetName) => cboSheet.Items.Add(sheetName);

        /// <summary>
        /// Selecciona una hoja del combo para pruebas automatizadas.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SelectSheetForTesting(int index) => cboSheet.SelectedIndex = index;

        /// <summary>
        /// Fija el rango de filas para pruebas automatizadas.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetRowRangeForTesting(int startRow, int endRow)
        {
            numStartRow.Value = startRow;
            numEndRow.Value = endRow;
        }

        /// <inheritdoc/>
        public async void Initialize(string fileName)
        {
            try
            {
                cboSheet.Items.Clear();
                var sheets = await Task.Run(() => ExcelSourceReaderFactory.GetSheets(fileName));
                if(sheets != null && sheets.Count > 0)
                {
                    cboSheet.Items.AddRange([.. sheets]);
                    cboSheet.SelectedIndex = 0;
                }
            }
            catch
            {
                //excepción silenciosa
            }

        }
    }
}
