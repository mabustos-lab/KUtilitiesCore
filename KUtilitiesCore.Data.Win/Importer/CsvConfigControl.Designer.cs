namespace KUtilitiesCore.Data.Win.Importer
{
    partial class CsvConfigControl
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.cboDelimiter = new System.Windows.Forms.ComboBox();
            this.lblDelim = new System.Windows.Forms.Label();
            this.cboEncoding = new System.Windows.Forms.ComboBox();
            this.lblEnc = new System.Windows.Forms.Label();
            this.chkHasHeader = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cboDelimiter
            // 
            this.cboDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDelimiter.FormattingEnabled = true;
            this.cboDelimiter.Location = new System.Drawing.Point(8, 21);
            this.cboDelimiter.Name = "cboDelimiter";
            this.cboDelimiter.Size = new System.Drawing.Size(100, 24);
            this.cboDelimiter.TabIndex = 0;
            // 
            // lblDelim
            // 
            this.lblDelim.AutoSize = true;
            this.lblDelim.Location = new System.Drawing.Point(5, 2);
            this.lblDelim.Name = "lblDelim";
            this.lblDelim.Size = new System.Drawing.Size(79, 16);
            this.lblDelim.TabIndex = 1;
            this.lblDelim.Text = "Delimitador:";
            // 
            // cboEncoding
            // 
            this.cboEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEncoding.FormattingEnabled = true;
            this.cboEncoding.Location = new System.Drawing.Point(114, 21);
            this.cboEncoding.Name = "cboEncoding";
            this.cboEncoding.Size = new System.Drawing.Size(109, 24);
            this.cboEncoding.TabIndex = 0;
            // 
            // lblEnc
            // 
            this.lblEnc.AutoSize = true;
            this.lblEnc.Location = new System.Drawing.Point(111, 2);
            this.lblEnc.Name = "lblEnc";
            this.lblEnc.Size = new System.Drawing.Size(67, 16);
            this.lblEnc.TabIndex = 1;
            this.lblEnc.Text = "Encoding:";
            // 
            // chkHasHeader
            // 
            this.chkHasHeader.AutoSize = true;
            this.chkHasHeader.Checked = true;
            this.chkHasHeader.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHasHeader.Location = new System.Drawing.Point(8, 51);
            this.chkHasHeader.Name = "chkHasHeader";
            this.chkHasHeader.Size = new System.Drawing.Size(150, 20);
            this.chkHasHeader.TabIndex = 2;
            this.chkHasHeader.Text = "Tiene encabezados";
            this.chkHasHeader.UseVisualStyleBackColor = true;
            // 
            // CsvConfigControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblDelim);
            this.Controls.Add(this.lblEnc);
            this.Controls.Add(this.chkHasHeader);
            this.Controls.Add(this.cboDelimiter);
            this.Controls.Add(this.cboEncoding);
            this.Name = "CsvConfigControl";
            this.Size = new System.Drawing.Size(300, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox cboDelimiter;
        private Label lblDelim;
        private ComboBox cboEncoding;
        private Label lblEnc;
        private CheckBox chkHasHeader;
    }
}
