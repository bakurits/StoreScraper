namespace Scraper.Controls
{
    partial class KeywordInputForm
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
            this.Tbx_Input = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Tbx_Input
            // 
            this.Tbx_Input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tbx_Input.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.Tbx_Input.ForeColor = System.Drawing.SystemColors.MenuHighlight;
            this.Tbx_Input.Location = new System.Drawing.Point(0, 0);
            this.Tbx_Input.Multiline = true;
            this.Tbx_Input.Name = "Tbx_Input";
            this.Tbx_Input.Size = new System.Drawing.Size(800, 450);
            this.Tbx_Input.TabIndex = 0;
            // 
            // KeywordInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Tbx_Input);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Name = "KeywordInputForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KeywordInputForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox Tbx_Input;
    }
}