namespace StoreScraper.Controls
{
    partial class ActionChooser
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
            this.CLbx_Actions = new System.Windows.Forms.CheckedListBox();
            this.Btn_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CLbx_Actions
            // 
            this.CLbx_Actions.FormattingEnabled = true;
            this.CLbx_Actions.Location = new System.Drawing.Point(186, 12);
            this.CLbx_Actions.Name = "CLbx_Actions";
            this.CLbx_Actions.Size = new System.Drawing.Size(175, 79);
            this.CLbx_Actions.TabIndex = 0;
            // 
            // Btn_OK
            // 
            this.Btn_OK.Location = new System.Drawing.Point(286, 94);
            this.Btn_OK.Name = "Btn_OK";
            this.Btn_OK.Size = new System.Drawing.Size(75, 23);
            this.Btn_OK.TabIndex = 1;
            this.Btn_OK.Text = "OK";
            this.Btn_OK.UseVisualStyleBackColor = true;
            this.Btn_OK.Click += new System.EventHandler(this.Btn_OK_Click);
            // 
            // ActionChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(366, 129);
            this.Controls.Add(this.Btn_OK);
            this.Controls.Add(this.CLbx_Actions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ActionChooser";
            this.Text = "ActionChoosercs";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox CLbx_Actions;
        private System.Windows.Forms.Button Btn_OK;
    }
}