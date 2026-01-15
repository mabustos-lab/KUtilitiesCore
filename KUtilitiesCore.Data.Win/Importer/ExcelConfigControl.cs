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
    public partial class ExcelConfigControl : UserControl, IImportConfigControl
    {
        public ExcelConfigControl()
        {
            InitializeComponent();
            cboSheet.SelectedIndexChanged += (a, b) => OnOptionsChanged();
            chkHasHeader.CheckedChanged += (a, b) => OnOptionsChanged();
        }
        private void OnOptionsChanged()
        {
            OptionsChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler OptionsChanged;

        public IParsingOptions GetParsingOptions()
        {
            return new ExcelParsingOptions
            {
                SheetName = cboSheet.SelectedText,
                HasHeader = chkHasHeader.Checked
            };
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
