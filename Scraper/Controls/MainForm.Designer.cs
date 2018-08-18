namespace StoreScraper.Controls
{
    partial class MainForm
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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.PGrid_Bot = new System.Windows.Forms.PropertyGrid();
            this.btn_FindProducts = new System.Windows.Forms.Button();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.Tab_Main = new System.Windows.Forms.TabPage();
            this.Btn_UrlMon = new System.Windows.Forms.Button();
            this.Tbx_Url = new System.Windows.Forms.TextBox();
            this.Clbx_Websites = new System.Windows.Forms.CheckedListBox();
            this.Btn_RemoveMon = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.CLbx_Monitor = new System.Windows.Forms.CheckedListBox();
            this.Btn_AddCriteriaToMonitor = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.Btn_Stop = new System.Windows.Forms.Button();
            this.label_FindingStatus = new System.Windows.Forms.Label();
            this.DGrid_FoundProducts = new System.Windows.Forms.DataGridView();
            this.Tab_Settings = new System.Windows.Forms.TabPage();
            this.Btn_Reset = new System.Windows.Forms.Button();
            this.Btn_SaveRestart = new System.Windows.Forms.Button();
            this.Btn_Save = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.RTbx_Proxies = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PGrid_Settings = new System.Windows.Forms.PropertyGrid();
            this.Tab_ErrorLog = new System.Windows.Forms.TabPage();
            this.Btn_ClearAllLogs = new System.Windows.Forms.Button();
            this.Rtbx_EventLog = new System.Windows.Forms.RichTextBox();
            this.Rtbx_DebugLog = new System.Windows.Forms.RichTextBox();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.SuspendLayout();
            this.Tabs.SuspendLayout();
            this.Tab_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGrid_FoundProducts)).BeginInit();
            this.Tab_Settings.SuspendLayout();
            this.Tab_ErrorLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(150, 150);
            this.toolStripContainer1.Location = new System.Drawing.Point(603, 421);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(150, 175);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(625, 406);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Size = new System.Drawing.Size(150, 100);
            this.splitContainer1.TabIndex = 7;
            // 
            // PGrid_Bot
            // 
            this.PGrid_Bot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PGrid_Bot.CanShowVisualStyleGlyphs = false;
            this.PGrid_Bot.HelpVisible = false;
            this.PGrid_Bot.Location = new System.Drawing.Point(459, 6);
            this.PGrid_Bot.Name = "PGrid_Bot";
            this.PGrid_Bot.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.PGrid_Bot.Size = new System.Drawing.Size(433, 192);
            this.PGrid_Bot.TabIndex = 2;
            this.PGrid_Bot.ToolbarVisible = false;
            // 
            // btn_FindProducts
            // 
            this.btn_FindProducts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_FindProducts.ForeColor = System.Drawing.Color.DarkCyan;
            this.btn_FindProducts.Location = new System.Drawing.Point(896, 6);
            this.btn_FindProducts.Name = "btn_FindProducts";
            this.btn_FindProducts.Size = new System.Drawing.Size(126, 23);
            this.btn_FindProducts.TabIndex = 3;
            this.btn_FindProducts.Text = "Test Search";
            this.btn_FindProducts.UseVisualStyleBackColor = true;
            this.btn_FindProducts.Click += new System.EventHandler(this.btn_FindProducts_Click);
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.Tab_Main);
            this.Tabs.Controls.Add(this.Tab_Settings);
            this.Tabs.Controls.Add(this.Tab_ErrorLog);
            this.Tabs.Location = new System.Drawing.Point(12, 12);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(1049, 620);
            this.Tabs.TabIndex = 7;
            // 
            // Tab_Main
            // 
            this.Tab_Main.Controls.Add(this.Btn_UrlMon);
            this.Tab_Main.Controls.Add(this.Tbx_Url);
            this.Tab_Main.Controls.Add(this.Clbx_Websites);
            this.Tab_Main.Controls.Add(this.Btn_RemoveMon);
            this.Tab_Main.Controls.Add(this.label5);
            this.Tab_Main.Controls.Add(this.CLbx_Monitor);
            this.Tab_Main.Controls.Add(this.Btn_AddCriteriaToMonitor);
            this.Tab_Main.Controls.Add(this.label3);
            this.Tab_Main.Controls.Add(this.Btn_Stop);
            this.Tab_Main.Controls.Add(this.label_FindingStatus);
            this.Tab_Main.Controls.Add(this.DGrid_FoundProducts);
            this.Tab_Main.Controls.Add(this.PGrid_Bot);
            this.Tab_Main.Controls.Add(this.btn_FindProducts);
            this.Tab_Main.Location = new System.Drawing.Point(4, 22);
            this.Tab_Main.Name = "Tab_Main";
            this.Tab_Main.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Main.Size = new System.Drawing.Size(1041, 594);
            this.Tab_Main.TabIndex = 0;
            this.Tab_Main.Text = "Main";
            this.Tab_Main.UseVisualStyleBackColor = true;
            // 
            // Btn_UrlMon
            // 
            this.Btn_UrlMon.Location = new System.Drawing.Point(914, 274);
            this.Btn_UrlMon.Name = "Btn_UrlMon";
            this.Btn_UrlMon.Size = new System.Drawing.Size(106, 23);
            this.Btn_UrlMon.TabIndex = 25;
            this.Btn_UrlMon.Text = "Monitor Url";
            this.Btn_UrlMon.UseVisualStyleBackColor = true;
            this.Btn_UrlMon.Click += new System.EventHandler(this.Btn_UrlMon_Click);
            // 
            // Tbx_Url
            // 
            this.Tbx_Url.Location = new System.Drawing.Point(526, 277);
            this.Tbx_Url.Name = "Tbx_Url";
            this.Tbx_Url.Size = new System.Drawing.Size(382, 20);
            this.Tbx_Url.TabIndex = 24;
            // 
            // Clbx_Websites
            // 
            this.Clbx_Websites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Clbx_Websites.FormattingEnabled = true;
            this.Clbx_Websites.Location = new System.Drawing.Point(154, 14);
            this.Clbx_Websites.Name = "Clbx_Websites";
            this.Clbx_Websites.Size = new System.Drawing.Size(299, 184);
            this.Clbx_Websites.TabIndex = 23;
            this.Clbx_Websites.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.Clbx_Websites_ItemCheck);
            this.Clbx_Websites.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Clbx_Websites_MouseUp);
            // 
            // Btn_RemoveMon
            // 
            this.Btn_RemoveMon.ForeColor = System.Drawing.Color.Crimson;
            this.Btn_RemoveMon.Location = new System.Drawing.Point(784, 547);
            this.Btn_RemoveMon.Name = "Btn_RemoveMon";
            this.Btn_RemoveMon.Size = new System.Drawing.Size(236, 23);
            this.Btn_RemoveMon.TabIndex = 22;
            this.Btn_RemoveMon.Text = "Remove Selected";
            this.Btn_RemoveMon.UseVisualStyleBackColor = true;
            this.Btn_RemoveMon.Click += new System.EventHandler(this.Btn_RemoveMon_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(746, 322);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 20);
            this.label5.TabIndex = 18;
            this.label5.Text = "Monitor";
            // 
            // CLbx_Monitor
            // 
            this.CLbx_Monitor.FormattingEnabled = true;
            this.CLbx_Monitor.Location = new System.Drawing.Point(526, 345);
            this.CLbx_Monitor.Name = "CLbx_Monitor";
            this.CLbx_Monitor.Size = new System.Drawing.Size(494, 199);
            this.CLbx_Monitor.TabIndex = 17;
            // 
            // Btn_AddCriteriaToMonitor
            // 
            this.Btn_AddCriteriaToMonitor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_AddCriteriaToMonitor.ForeColor = System.Drawing.Color.DarkGreen;
            this.Btn_AddCriteriaToMonitor.Location = new System.Drawing.Point(896, 64);
            this.Btn_AddCriteriaToMonitor.Name = "Btn_AddCriteriaToMonitor";
            this.Btn_AddCriteriaToMonitor.Size = new System.Drawing.Size(126, 40);
            this.Btn_AddCriteriaToMonitor.TabIndex = 14;
            this.Btn_AddCriteriaToMonitor.Text = "Add Search Criteria to Monitor";
            this.Btn_AddCriteriaToMonitor.UseVisualStyleBackColor = true;
            this.Btn_AddCriteriaToMonitor.Click += new System.EventHandler(this.Btn_AddToMonitor_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.DimGray;
            this.label3.Location = new System.Drawing.Point(17, 578);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(195, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Double click product to open in browser";
            // 
            // Btn_Stop
            // 
            this.Btn_Stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Stop.ForeColor = System.Drawing.Color.Crimson;
            this.Btn_Stop.Location = new System.Drawing.Point(896, 35);
            this.Btn_Stop.Name = "Btn_Stop";
            this.Btn_Stop.Size = new System.Drawing.Size(126, 23);
            this.Btn_Stop.TabIndex = 7;
            this.Btn_Stop.Text = "Stop";
            this.Btn_Stop.UseVisualStyleBackColor = true;
            this.Btn_Stop.Click += new System.EventHandler(this.Btn_Stop_Click);
            // 
            // label_FindingStatus
            // 
            this.label_FindingStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_FindingStatus.AutoSize = true;
            this.label_FindingStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label_FindingStatus.Location = new System.Drawing.Point(892, 113);
            this.label_FindingStatus.Name = "label_FindingStatus";
            this.label_FindingStatus.Size = new System.Drawing.Size(0, 20);
            this.label_FindingStatus.TabIndex = 6;
            // 
            // DGrid_FoundProducts
            // 
            this.DGrid_FoundProducts.AllowUserToAddRows = false;
            this.DGrid_FoundProducts.AllowUserToResizeColumns = false;
            this.DGrid_FoundProducts.AllowUserToResizeRows = false;
            this.DGrid_FoundProducts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DGrid_FoundProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.DGrid_FoundProducts.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.DGrid_FoundProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGrid_FoundProducts.Location = new System.Drawing.Point(20, 226);
            this.DGrid_FoundProducts.Name = "DGrid_FoundProducts";
            this.DGrid_FoundProducts.ReadOnly = true;
            this.DGrid_FoundProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGrid_FoundProducts.Size = new System.Drawing.Size(500, 349);
            this.DGrid_FoundProducts.TabIndex = 5;
            this.DGrid_FoundProducts.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ProductGrids_OnDoubleClick);
            this.DGrid_FoundProducts.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Grids_CellFormatting);
            // 
            // Tab_Settings
            // 
            this.Tab_Settings.Controls.Add(this.Btn_Reset);
            this.Tab_Settings.Controls.Add(this.Btn_SaveRestart);
            this.Tab_Settings.Controls.Add(this.Btn_Save);
            this.Tab_Settings.Controls.Add(this.label4);
            this.Tab_Settings.Controls.Add(this.RTbx_Proxies);
            this.Tab_Settings.Controls.Add(this.label2);
            this.Tab_Settings.Controls.Add(this.PGrid_Settings);
            this.Tab_Settings.Location = new System.Drawing.Point(4, 22);
            this.Tab_Settings.Name = "Tab_Settings";
            this.Tab_Settings.Padding = new System.Windows.Forms.Padding(3);
            this.Tab_Settings.Size = new System.Drawing.Size(1041, 594);
            this.Tab_Settings.TabIndex = 1;
            this.Tab_Settings.Text = "Settings";
            this.Tab_Settings.UseVisualStyleBackColor = true;
            // 
            // Btn_Reset
            // 
            this.Btn_Reset.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Btn_Reset.Location = new System.Drawing.Point(478, 226);
            this.Btn_Reset.Name = "Btn_Reset";
            this.Btn_Reset.Size = new System.Drawing.Size(131, 23);
            this.Btn_Reset.TabIndex = 10;
            this.Btn_Reset.Text = "Reset Settings";
            this.Btn_Reset.UseVisualStyleBackColor = true;
            this.Btn_Reset.Click += new System.EventHandler(this.Btn_Reset_Click);
            // 
            // Btn_SaveRestart
            // 
            this.Btn_SaveRestart.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Btn_SaveRestart.Location = new System.Drawing.Point(615, 226);
            this.Btn_SaveRestart.Name = "Btn_SaveRestart";
            this.Btn_SaveRestart.Size = new System.Drawing.Size(139, 23);
            this.Btn_SaveRestart.TabIndex = 9;
            this.Btn_SaveRestart.Text = "Save And Restart";
            this.Btn_SaveRestart.UseVisualStyleBackColor = true;
            this.Btn_SaveRestart.Click += new System.EventHandler(this.Btn_SaveRestart_Click);
            // 
            // Btn_Save
            // 
            this.Btn_Save.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Btn_Save.Location = new System.Drawing.Point(760, 226);
            this.Btn_Save.Name = "Btn_Save";
            this.Btn_Save.Size = new System.Drawing.Size(88, 23);
            this.Btn_Save.TabIndex = 8;
            this.Btn_Save.Text = "Save";
            this.Btn_Save.UseVisualStyleBackColor = true;
            this.Btn_Save.Click += new System.EventHandler(this.Btn_Save_Click);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(635, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Proxies";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // RTbx_Proxies
            // 
            this.RTbx_Proxies.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.RTbx_Proxies.Location = new System.Drawing.Point(478, 38);
            this.RTbx_Proxies.Multiline = true;
            this.RTbx_Proxies.Name = "RTbx_Proxies";
            this.RTbx_Proxies.Size = new System.Drawing.Size(370, 183);
            this.RTbx_Proxies.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(201, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Global Settings";
            // 
            // PGrid_Settings
            // 
            this.PGrid_Settings.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PGrid_Settings.CommandsVisibleIfAvailable = false;
            this.PGrid_Settings.HelpVisible = false;
            this.PGrid_Settings.Location = new System.Drawing.Point(98, 38);
            this.PGrid_Settings.Name = "PGrid_Settings";
            this.PGrid_Settings.Size = new System.Drawing.Size(374, 183);
            this.PGrid_Settings.TabIndex = 4;
            this.PGrid_Settings.ToolbarVisible = false;
            // 
            // Tab_ErrorLog
            // 
            this.Tab_ErrorLog.Controls.Add(this.Btn_ClearAllLogs);
            this.Tab_ErrorLog.Controls.Add(this.Rtbx_EventLog);
            this.Tab_ErrorLog.Controls.Add(this.Rtbx_DebugLog);
            this.Tab_ErrorLog.Location = new System.Drawing.Point(4, 22);
            this.Tab_ErrorLog.Name = "Tab_ErrorLog";
            this.Tab_ErrorLog.Size = new System.Drawing.Size(1041, 594);
            this.Tab_ErrorLog.TabIndex = 2;
            this.Tab_ErrorLog.Text = "Error Log";
            this.Tab_ErrorLog.UseVisualStyleBackColor = true;
            // 
            // Btn_ClearAllLogs
            // 
            this.Btn_ClearAllLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.Btn_ClearAllLogs.ForeColor = System.Drawing.Color.Red;
            this.Btn_ClearAllLogs.Location = new System.Drawing.Point(743, 269);
            this.Btn_ClearAllLogs.Name = "Btn_ClearAllLogs";
            this.Btn_ClearAllLogs.Size = new System.Drawing.Size(283, 32);
            this.Btn_ClearAllLogs.TabIndex = 2;
            this.Btn_ClearAllLogs.Text = "Clear All Logs";
            this.Btn_ClearAllLogs.UseVisualStyleBackColor = true;
            this.Btn_ClearAllLogs.Click += new System.EventHandler(this.Btn_ClearAllLogs_Click);
            // 
            // Rtbx_EventLog
            // 
            this.Rtbx_EventLog.ForeColor = System.Drawing.Color.DarkRed;
            this.Rtbx_EventLog.Location = new System.Drawing.Point(3, 3);
            this.Rtbx_EventLog.Name = "Rtbx_EventLog";
            this.Rtbx_EventLog.Size = new System.Drawing.Size(1023, 260);
            this.Rtbx_EventLog.TabIndex = 1;
            this.Rtbx_EventLog.Text = "";
            // 
            // Rtbx_DebugLog
            // 
            this.Rtbx_DebugLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Rtbx_DebugLog.Location = new System.Drawing.Point(0, 329);
            this.Rtbx_DebugLog.Name = "Rtbx_DebugLog";
            this.Rtbx_DebugLog.Size = new System.Drawing.Size(1041, 265);
            this.Rtbx_DebugLog.TabIndex = 0;
            this.Rtbx_DebugLog.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 657);
            this.Controls.Add(this.Tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Name = "MainForm";
            this.Text = "Scraper";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.Tabs.ResumeLayout(false);
            this.Tab_Main.ResumeLayout(false);
            this.Tab_Main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGrid_FoundProducts)).EndInit();
            this.Tab_Settings.ResumeLayout(false);
            this.Tab_Settings.PerformLayout();
            this.Tab_ErrorLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PropertyGrid PGrid_Bot;
        private System.Windows.Forms.Button btn_FindProducts;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage Tab_Main;
        private System.Windows.Forms.TabPage Tab_Settings;
        private System.Windows.Forms.PropertyGrid PGrid_Settings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView DGrid_FoundProducts;
        private System.Windows.Forms.Label label_FindingStatus;
        private System.Windows.Forms.Button Btn_Stop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage Tab_ErrorLog;
        private System.Windows.Forms.RichTextBox Rtbx_DebugLog;
        private System.Windows.Forms.RichTextBox Rtbx_EventLog;
        private System.Windows.Forms.Button Btn_AddCriteriaToMonitor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox RTbx_Proxies;
        private System.Windows.Forms.Button Btn_Save;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckedListBox CLbx_Monitor;
        private System.Windows.Forms.Button Btn_RemoveMon;
        private System.Windows.Forms.Button Btn_SaveRestart;
        private System.Windows.Forms.Button Btn_ClearAllLogs;
        private System.Windows.Forms.CheckedListBox Clbx_Websites;
        private System.Windows.Forms.Button Btn_Reset;
        private System.Windows.Forms.Button Btn_UrlMon;
        private System.Windows.Forms.TextBox Tbx_Url;
    }
}

