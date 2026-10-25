using DocumentFormat.OpenXml.Math;
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
    /// Control de configuración para archivos de texto delimitados (CSV/TSV/PSV).
    /// Permite elegir separador, codificación, presencia de cabecera, recorte de
    /// espacios y el tratamiento de líneas vacías; el asistente de importación lo
    /// hospeda cuando el archivo seleccionado no es Excel.
    /// </summary>
    public partial class CsvConfigControl : UserControl, IImportConfigControl
    {

        /// <summary>
        /// Inicializa el control y precarga las opciones de separador y codificación.
        /// </summary>
        public CsvConfigControl()
        {
            InitializeComponent();
            InitData();
        }

        #region Events

        /// <inheritdoc/>
        public event EventHandler? OptionsChanged;

        #endregion Events

        #region Methods

        /// <inheritdoc/>
        public IParsingOptions GetParsingOptions()
        {
            return new TextFileParsingOptions
            {
                Separator = cboDelimiter.SelectedValue?.ToString() ?? ",",
                Encoding = (Encoding)(cboEncoding.SelectedValue ?? Encoding.UTF8),
                HasHeader = chkHasHeader.Checked,
                TrimValues = chkTrimValues.Checked,
                IgnoreEmptyLines = chkIgnoreEmptyLines.Checked
            };
        }

        /// <inheritdoc/>
        public void Initialize(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string ext = Path.GetExtension(fileName).ToLower();

            // Autoconfiguración básica basada en extensión
            if (ext == ".tsv")
            {
                cboDelimiter.SelectedValue = "\t";
            }
            else if (ext == ".psv")
            {
                cboDelimiter.SelectedValue = "|";
            }
            
        }

        private void InitData()
        {
            cboDelimiter.DataSource = new List<KeyValuePair <string, string>>()
            { new( "{comma}", "," ),new( "{dot-comma}", ";" ),new( "{pipe}", "|" ), new("{tab}", "\t")};
            cboDelimiter.DisplayMember = $"{nameof(KeyValuePair<string, string>.Key)}";
            cboDelimiter.ValueMember = $"{nameof(KeyValuePair<string, string>.Value)}";
            cboDelimiter.SelectedIndex = 0;
            cboEncoding.DataSource = new List<KeyValuePair<string, Encoding>>()
            { new( "UTF-8", Encoding.UTF8), new ( "UTF-8 con BOM", new UTF8Encoding(true)),
            new ( "UTF-16 LE", Encoding.Unicode), new("UTF-16 BE", Encoding.BigEndianUnicode)};
            cboEncoding.DisplayMember = $"{nameof(KeyValuePair<string, Encoding>.Key)}";
            cboEncoding.ValueMember = $"{nameof(KeyValuePair<string, Encoding>.Value)}";
            cboEncoding.SelectedIndex = 0;
            cboDelimiter.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            cboEncoding.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            chkHasHeader.CheckedChanged += (a, b) => OnOptionsChanged();
            chkTrimValues.CheckedChanged += (a, b) => OnOptionsChanged();
            chkIgnoreEmptyLines.CheckedChanged += (a, b) => OnOptionsChanged();
        }

        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fija el estado de "Quitar espacios en los valores" para pruebas automatizadas.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTrimValuesForTesting(bool value) => chkTrimValues.Checked = value;

        /// <summary>
        /// Fija el estado de "Ignorar líneas vacías" para pruebas automatizadas.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetIgnoreEmptyLinesForTesting(bool value) => chkIgnoreEmptyLines.Checked = value;

        #endregion Methods
    }
}