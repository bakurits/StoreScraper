namespace ScraperTest.Controls
{
    partial class EOTestForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.webControl1 = new EO.WinForm.WebControl();
            this.Driver = new EO.WebBrowser.WebView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.webControl2 = new EO.WinForm.WebControl();
            this.Driver2 = new EO.WebBrowser.WebView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.webControl3 = new EO.WinForm.WebControl();
            this.Driver3 = new EO.WebBrowser.WebView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 450);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.tabPage1.Controls.Add(this.webControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // webControl1
            // 
            this.webControl1.BackColor = System.Drawing.Color.White;
            this.webControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webControl1.Location = new System.Drawing.Point(3, 3);
            this.webControl1.Name = "webControl1";
            this.webControl1.Size = new System.Drawing.Size(786, 418);
            this.webControl1.TabIndex = 0;
            this.webControl1.Text = "webControl1";
            this.webControl1.WebView = this.Driver;
            // 
            // Driver
            // 
            this.Driver.ObjectForScripting = null;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.webControl2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 424);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // webControl2
            // 
            this.webControl2.BackColor = System.Drawing.Color.White;
            this.webControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webControl2.Location = new System.Drawing.Point(3, 3);
            this.webControl2.Name = "webControl2";
            this.webControl2.Size = new System.Drawing.Size(786, 418);
            this.webControl2.TabIndex = 0;
            this.webControl2.Text = "webControl2";
            this.webControl2.WebView = this.Driver2;
            // 
            // Driver2
            // 
            this.Driver2.ObjectForScripting = null;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.webControl3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(792, 424);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // webControl3
            // 
            this.webControl3.BackColor = System.Drawing.Color.White;
            this.webControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webControl3.Location = new System.Drawing.Point(3, 3);
            this.webControl3.Name = "webControl3";
            this.webControl3.Size = new System.Drawing.Size(786, 418);
            this.webControl3.TabIndex = 0;
            this.webControl3.Text = "webControl3";
            this.webControl3.WebView = this.Driver3;
            // 
            // Driver3
            // 
            this.Driver3.ObjectForScripting = null;
            // 
            // EOTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "EOTestForm";
            this.Text = "EOTestForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        public EO.WebBrowser.WebView Driver;
        public EO.WebBrowser.WebView Driver2;
        public EO.WinForm.WebControl webControl1;
        private EO.WinForm.WebControl webControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private EO.WinForm.WebControl webControl3;
        public EO.WebBrowser.WebView Driver3;
    }
}