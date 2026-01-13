namespace KUtilitiesCore.Data.Win.Importer
{
    partial class ImportWizardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                OnDispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblType = new System.Windows.Forms.Label();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.pnlConfig = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvMapping = new System.Windows.Forms.DataGridView();
            this.DestinationField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SourceColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.tabControlResults = new System.Windows.Forms.TabControl();
            this.tabData = new System.Windows.Forms.TabPage();
            this.dgvPreview = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslWarning = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabErrors = new System.Windows.Forms.TabPage();
            this.dgvErrors = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.cbFilterHasError = new System.Windows.Forms.CheckBox();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).BeginInit();
            this.tabControlResults.SuspendLayout();
            this.tabData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tabErrors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblType);
            this.pnlTop.Controls.Add(this.btnLoadData);
            this.pnlTop.Controls.Add(this.btnBrowse);
            this.pnlTop.Controls.Add(this.txtFilePath);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTop.Size = new System.Drawing.Size(982, 60);
            this.pnlTop.TabIndex = 0;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 4);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(55, 16);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "Archivo:";
            // 
            // btnLoadData
            // 
            this.btnLoadData.Enabled = false;
            this.btnLoadData.Location = new System.Drawing.Point(669, 23);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(150, 24);
            this.btnLoadData.TabIndex = 1;
            this.btnLoadData.Text = "Cargar";
            this.btnLoadData.UseVisualStyleBackColor = true;
            this.btnLoadData.Click += new System.EventHandler(this.btnLoadData_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(623, 24);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(40, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtFilePath.Location = new System.Drawing.Point(15, 25);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Size = new System.Drawing.Size(602, 22);
            this.txtFilePath.TabIndex = 1;
            // 
            // pnlConfig
            // 
            this.pnlConfig.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlConfig.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlConfig.Location = new System.Drawing.Point(0, 60);
            this.pnlConfig.Name = "pnlConfig";
            this.pnlConfig.Size = new System.Drawing.Size(982, 100);
            this.pnlConfig.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 160);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlResults);
            this.splitContainer1.Size = new System.Drawing.Size(982, 443);
            this.splitContainer1.SplitterDistance = 249;
            this.splitContainer1.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgvMapping);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(249, 443);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mapeo de Campos";
            // 
            // dgvMapping
            // 
            this.dgvMapping.AllowUserToAddRows = false;
            this.dgvMapping.AllowUserToDeleteRows = false;
            this.dgvMapping.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dgvMapping.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMapping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DestinationField,
            this.SourceColumn});
            this.dgvMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMapping.Location = new System.Drawing.Point(3, 18);
            this.dgvMapping.Name = "dgvMapping";
            this.dgvMapping.RowHeadersVisible = false;
            this.dgvMapping.RowHeadersWidth = 51;
            this.dgvMapping.RowTemplate.Height = 24;
            this.dgvMapping.Size = new System.Drawing.Size(243, 422);
            this.dgvMapping.TabIndex = 0;
            // 
            // DestinationField
            // 
            this.DestinationField.HeaderText = "Campo Destino";
            this.DestinationField.MinimumWidth = 6;
            this.DestinationField.Name = "DestinationField";
            this.DestinationField.ReadOnly = true;
            this.DestinationField.Width = 120;
            // 
            // SourceColumn
            // 
            this.SourceColumn.HeaderText = "Columna Origen (Archivo)";
            this.SourceColumn.MinimumWidth = 6;
            this.SourceColumn.Name = "SourceColumn";
            this.SourceColumn.Width = 120;
            // 
            // tabControlResults
            // 
            this.tabControlResults.Controls.Add(this.tabData);
            this.tabControlResults.Controls.Add(this.tabErrors);
            this.tabControlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlResults.Location = new System.Drawing.Point(0, 0);
            this.tabControlResults.Name = "tabControlResults";
            this.tabControlResults.SelectedIndex = 0;
            this.tabControlResults.Size = new System.Drawing.Size(729, 443);
            this.tabControlResults.TabIndex = 0;
            // 
            // tabData
            // 
            this.tabData.Controls.Add(this.dgvPreview);
            this.tabData.Controls.Add(this.statusStrip1);
            this.tabData.Location = new System.Drawing.Point(4, 25);
            this.tabData.Name = "tabData";
            this.tabData.Padding = new System.Windows.Forms.Padding(3);
            this.tabData.Size = new System.Drawing.Size(721, 414);
            this.tabData.TabIndex = 0;
            this.tabData.Text = "Datos Importados (Editable)";
            this.tabData.UseVisualStyleBackColor = true;
            // 
            // dgvPreview
            // 
            this.dgvPreview.AllowUserToAddRows = false;
            this.dgvPreview.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dgvPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvPreview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPreview.Location = new System.Drawing.Point(3, 3);
            this.dgvPreview.Name = "dgvPreview";
            this.dgvPreview.RowHeadersVisible = false;
            this.dgvPreview.RowHeadersWidth = 51;
            this.dgvPreview.RowTemplate.Height = 24;
            this.dgvPreview.Size = new System.Drawing.Size(715, 382);
            this.dgvPreview.TabIndex = 0;
            this.dgvPreview.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvPreview_CellEndEdit);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslCount,
            this.tsslWarning});
            this.statusStrip1.Location = new System.Drawing.Point(3, 385);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(715, 26);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslCount
            // 
            this.tsslCount.Name = "tsslCount";
            this.tsslCount.Size = new System.Drawing.Size(117, 20);
            this.tsslCount.Text = "Filas cargadas: 0";
            // 
            // tsslWarning
            // 
            this.tsslWarning.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsslWarning.Image = global::KUtilitiesCore.Data.Win.Properties.Resources.Warn16x16;
            this.tsslWarning.Name = "tsslWarning";
            this.tsslWarning.Size = new System.Drawing.Size(193, 20);
            this.tsslWarning.Text = "Se encontraron errores.";
            this.tsslWarning.Visible = false;
            // 
            // tabErrors
            // 
            this.tabErrors.Controls.Add(this.dgvErrors);
            this.tabErrors.Location = new System.Drawing.Point(4, 25);
            this.tabErrors.Name = "tabErrors";
            this.tabErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabErrors.Size = new System.Drawing.Size(721, 414);
            this.tabErrors.TabIndex = 1;
            this.tabErrors.Text = "Errores de Validación";
            this.tabErrors.UseVisualStyleBackColor = true;
            // 
            // dgvErrors
            // 
            this.dgvErrors.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dgvErrors.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvErrors.Location = new System.Drawing.Point(3, 3);
            this.dgvErrors.Name = "dgvErrors";
            this.dgvErrors.RowHeadersWidth = 51;
            this.dgvErrors.RowTemplate.Height = 24;
            this.dgvErrors.Size = new System.Drawing.Size(715, 408);
            this.dgvErrors.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbFilterHasError);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnImport);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 603);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(982, 50);
            this.panel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(711, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(97, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            this.btnImport.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnImport.Enabled = false;
            this.btnImport.Location = new System.Drawing.Point(814, 9);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(156, 32);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Aceptar e Importar";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // cbFilterHasError
            // 
            this.cbFilterHasError.AutoSize = true;
            this.cbFilterHasError.Location = new System.Drawing.Point(11, 16);
            this.cbFilterHasError.Name = "cbFilterHasError";
            this.cbFilterHasError.Size = new System.Drawing.Size(204, 20);
            this.cbFilterHasError.TabIndex = 2;
            this.cbFilterHasError.Text = "Mostrar solo filas con errores.";
            this.cbFilterHasError.UseVisualStyleBackColor = true;
            this.cbFilterHasError.CheckedChanged += new System.EventHandler(this.cbFilterHasError_CheckedChanged);
            // 
            // ImportWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 653);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlConfig);
            this.Controls.Add(this.pnlTop);
            this.Name = "ImportWizardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Asistente de Importación de Datos";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMapping)).EndInit();
            this.tabControlResults.ResumeLayout(false);
            this.tabData.ResumeLayout(false);
            this.tabData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPreview)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabErrors.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Panel pnlTop;
        private TextBox txtFilePath;
        private Button btnBrowse;
        private Button btnLoadData;
        private Label lblType;
        private Panel pnlConfig;
        private SplitContainer splitContainer1;
        private GroupBox groupBox1;
        private TabControl tabControlResults;
        private TabPage tabData;
        private TabPage tabErrors;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslCount;
        private DataGridView dgvErrors;
        private Panel panel1;
        private ToolStripStatusLabel tsslWarning;
        private Button btnCancel;
        private Button btnImport;
        private DataGridViewTextBoxColumn DestinationField;
        private DataGridViewComboBoxColumn SourceColumn;
        protected DataGridView dgvMapping;
        protected DataGridView dgvPreview;
        private CheckBox cbFilterHasError;
    }
}