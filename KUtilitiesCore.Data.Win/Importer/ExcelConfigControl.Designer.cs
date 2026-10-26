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
            this.lblStartRow = new System.Windows.Forms.Label();
            this.numStartRow = new System.Windows.Forms.NumericUpDown();
            this.lblEndRow = new System.Windows.Forms.Label();
            this.numEndRow = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numStartRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndRow)).BeginInit();
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
            this.cboSheet.Size = new System.Drawing.Size(253, 24);
            this.cboSheet.TabIndex = 3;
            // 
            // lblStartRow
            // 
            this.lblStartRow.AutoSize = true;
            this.lblStartRow.Location = new System.Drawing.Point(3, 79);
            this.lblStartRow.Name = "lblStartRow";
            this.lblStartRow.Size = new System.Drawing.Size(70, 16);
            this.lblStartRow.TabIndex = 6;
            this.lblStartRow.Text = "Fila inicial:";
            // 
            // numStartRow
            // 
            this.numStartRow.Location = new System.Drawing.Point(76, 76);
            this.numStartRow.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numStartRow.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numStartRow.Name = "numStartRow";
            this.numStartRow.Size = new System.Drawing.Size(50, 23);
            this.numStartRow.TabIndex = 7;
            this.numStartRow.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblEndRow
            // 
            this.lblEndRow.AutoSize = true;
            this.lblEndRow.Location = new System.Drawing.Point(140, 79);
            this.lblEndRow.Name = "lblEndRow";
            this.lblEndRow.Size = new System.Drawing.Size(129, 16);
            this.lblEndRow.TabIndex = 8;
            this.lblEndRow.Text = "Fila final (0 = todas):";
            // 
            // numEndRow
            // 
            this.numEndRow.Location = new System.Drawing.Point(280, 76);
            this.numEndRow.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numEndRow.Name = "numEndRow";
            this.numEndRow.Size = new System.Drawing.Size(50, 23);
            this.numEndRow.TabIndex = 9;
            // 
            // ExcelConfigControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblSheet);
            this.Controls.Add(this.chkHasHeader);
            this.Controls.Add(this.cboSheet);
            this.Controls.Add(this.lblStartRow);
            this.Controls.Add(this.numStartRow);
            this.Controls.Add(this.lblEndRow);
            this.Controls.Add(this.numEndRow);
            this.Name = "ExcelConfigControl";
            this.Size = new System.Drawing.Size(420, 100);
            ((System.ComponentModel.ISupportInitialize)(this.numStartRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEndRow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblSheet;
        private CheckBox chkHasHeader;
        private ComboBox cboSheet;
        private Label lblStartRow;
        private NumericUpDown numStartRow;
        private Label lblEndRow;
        private NumericUpDown numEndRow;
    }
}
