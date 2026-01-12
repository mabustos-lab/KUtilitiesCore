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
    public partial class CsvConfigControl : UserControl, IImportConfigControl
    {

        public CsvConfigControl()
        {
            InitializeComponent();
            InitData();
        }

        #region Events

        public event EventHandler OptionsChanged;

        #endregion Events

        #region Methods

        public IParsingOptions GetParsingOptions()
        {
            return new TextFileParsingOptions
            {
                Separator = cboDelimiter.SelectedValue?.ToString(),
                Encoding = (Encoding)(cboEncoding.SelectedValue ?? Encoding.UTF8),
                HasHeader = chkHasHeader.Checked
            };
        }

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
            cboDelimiter.DataSource = new Dictionary<string, string>
            { { ",", "," }, { ";", ";" }, { "|", "|" }, { "{Tab}", "\t" } };
            cboDelimiter.DisplayMember = $"{nameof(Dictionary<string, string>.Keys)}";
            cboDelimiter.ValueMember = $"{nameof(Dictionary<string, string>.Values)}";
            cboDelimiter.SelectedIndex = 1;
            cboEncoding.DataSource = new Dictionary<string, Encoding>
            { { "UTF-8", Encoding.UTF8}, { "UTF-8 con BOM", new UTF8Encoding(true)},
            { "UTF-16 LE", Encoding.Unicode}, { "UTF-16 BE", Encoding.BigEndianUnicode}};
            cboEncoding.DisplayMember = $"{nameof(Dictionary<string, string>.Keys)}";
            cboEncoding.ValueMember = $"{nameof(Dictionary<string, string>.Values)}";
            cboEncoding.SelectedIndex = 1;
            cboDelimiter.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            cboEncoding.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            chkHasHeader.CheckedChanged += (a, b) => OnOptionsChanged();
        }

        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion Methods
    }
}