namespace KUtilitiesCore.Data.Win.Importer
{
    partial class ExcelConfigControl
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
            this.lblSheet = new System.Windows.Forms.Label();
            this.chkHasHeader = new System.Windows.Forms.CheckBox();
            this.cboSheet = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblSheet
            // 
            this.lblSheet.AutoSize = true;
            this.lblSheet.Location = new System.Drawing.Point(3, 3);
            this.lblSheet.Name = "lblSheet";
            this.lblSheet.Size = new System.Drawing.Size(39, 16);
            this.lblSheet.TabIndex = 4;
            this.lblSheet.Text = "Hoja:";
            // 
            // chkHasHeader
            // 
            this.chkHasHeader.AutoSize = true;
            this.chkHasHeader.Checked = true;
            this.chkHasHeader.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHasHeader.Location = new System.Drawing.Point(3, 52);
            this.chkHasHeader.Name = "chkHasHeader";
            this.chkHasHeader.Size = new System.Drawing.Size(150, 20);
            this.chkHasHeader.TabIndex = 5;
            this.chkHasHeader.Text = "Tiene encabezados";
            this.chkHasHeader.UseVisualStyleBackColor = true;
            // 
            // cboSheet
            // 
            this.cboSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheet.FormattingEnabled = true;
            this.cboSheet.Location = new System.Drawing.Point(3, 22);
            this.cboSheet.Name = "cboSheet";
            this.cboSheet.Size = new System.Drawing.Size(100, 24);
            this.cboSheet.TabIndex = 3;
            // 
            // ExcelConfigControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblSheet);
            this.Controls.Add(this.chkHasHeader);
            this.Controls.Add(this.cboSheet);
            this.Name = "ExcelConfigControl";
            this.Size = new System.Drawing.Size(300, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblSheet;
        private CheckBox chkHasHeader;
        private ComboBox cboSheet;
    }
}
