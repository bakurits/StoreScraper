using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http;
using StoreScraper.Models;

namespace StoreScraper.Controls
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource _findTokenSource = new CancellationTokenSource();
        private BindingList<Product> _listOfProducts = new BindingList<Product>();
        private CancellationTokenSource _monitorCancellation = new CancellationTokenSource();

        public MainForm()
        {
            InitializeComponent();
            DGrid_FoundProducts.DataSource = _listOfProducts;
            Cbx_ChooseStore.Items.AddRange(AppSettings.Default.AvaibleBots.ToArray());
            PGrid_Settings.SelectedObject = AppSettings.Default;
            RTbx_Proxies.Text = string.Join("\r\n", AppSettings.Default.Proxies);
            Logger.Instance.OnLogged += (message, color) => Rtbx_EventLog.AppendText(message,color);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
        }

        private void btn_FindProducts_Click(object sender, EventArgs e)
        {
            _findTokenSource = new CancellationTokenSource();
            _listOfProducts.Clear();
            DGrid_FoundProducts.Refresh();
            btn_FindProducts.Enabled = false;
            label_FindingStatus.ForeColor = Color.SaddleBrown;
            label_FindingStatus.Text = @"Processing...";

            var scraper = Cbx_ChooseStore.SelectedItem as ScraperBase;

            Task.Run(() => FindAction(scraper)).ContinueWith(FindProductsTaskCompleted);

        }

        private void FindAction(ScraperBase scraper)
        {
            scraper.FindItems(out var products, PGrid_Bot.SelectedObject as SearchSettingsBase, _findTokenSource.Token);
            DGrid_FoundProducts.Invoke(new Action(() =>
            {
                lock (_listOfProducts)
                {
                    products.ForEach(product => _listOfProducts.Add(product));
                    _findTokenSource.Token.ThrowIfCancellationRequested();
                }
            }));
        }

        private void FindProductsTaskCompleted(Task task)
        {
            DGrid_FoundProducts.Invoke(new Action(() =>
            {
                if (task.IsFaulted)
                {
                    Rtbx_DebugLog.AppendText(task.Exception?.StackTrace + "\r\n");
                    label_FindingStatus.Text = @"Some Errors!";
                    label_FindingStatus.ForeColor = Color.Red;
                }
                else
                {
                    label_FindingStatus.Text = @"Done!";
                    label_FindingStatus.ForeColor = Color.DarkGreen;
                }

                btn_FindProducts.Enabled = true;
            }));


        }

        private void Btn_Stop_Click(object sender, EventArgs e)
        {
            _findTokenSource.Cancel();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            Tabs.SetBounds(0, 0, this.Width, this.Height);
        }

        private void Btn_SelectAllProducts_Click(object sender, EventArgs e)
        {
            DGrid_FoundProducts.ColumnHeadersVisible = false;
            DGrid_FoundProducts.SelectAll();
            DGrid_FoundProducts.ColumnHeadersVisible = true;
        }

        private void ProductGrids_OnDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;

            var product = grid.Rows[e.RowIndex].DataBoundItem as Product;

            Process.Start(product.Url);
        }

        private void Grids_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            cell.ToolTipText = "Double click to open in browser";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            AppSettings.Default.Save();
            CookieCollector.Default.Dispose();
            var processes = Process.GetProcessesByName("firefox");
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    //ingored
                }
            }
            Environment.Exit(Environment.ExitCode);
        }

        private void Cbx_ChooseStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bot = (sender as ComboBox).SelectedItem as ScraperBase;
            var sType = bot.SearchSettings;
            PGrid_Bot.SelectedObject = Activator.CreateInstance(sType);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            AppSettings.Default.Proxies = RTbx_Proxies.Text.Replace("\r", "").Split('\n').ToList().ConvertAll<string>(proxy => 
            {
                if (!proxy.Contains("http://") && !proxy.Contains("https://"))
                {
                    return "http://" + proxy;
                }
                else return proxy;
            });

            AppSettings.Default.Save();
        }

        private void Btn_AddToMonitor_Click(object sender, EventArgs e)
        {
            var storeIndex = Cbx_ChooseStore.SelectedIndex;
            var searchOptions = (SearchSettingsBase)PGrid_Bot.SelectedObject;

            ActionChooser form = new ActionChooser();
            form.ShowDialog();
            var actions = new List<MonitoringTask.FinalAction>();

            if (form.DialogResult == DialogResult.OK)
            {
                actions.AddRange(form.GetFinalActions());
            }
            else return;

            var scraper = AppSettings.Default.AvaibleBots[storeIndex];
            List<Product> curProductsList = null;
            try
            {
                scraper.FindItems(out curProductsList, searchOptions, CancellationToken.None);
            }
            catch
            {
                MessageBox.Show($"Error Occured while trying to obtain current products with specified search criteria on {scraper.WebsiteName}");

                return;
            }

            var item = new MonitoringTask()
            {
                SearchSettings = searchOptions,
                Bot = scraper,
                FinalActions = actions,
                OldItems = curProductsList
            };

            MessageBox.Show(@"searching filter is now adding to monitor list. 
                              That may take several mins...
                              When Complete you will see filter added in monitor");


            Task.Run(() =>
            {

                item.Bot.Active = true;
                CLbx_Monitor.Invoke(new Action(() => CLbx_Monitor.Items.Add(item, true)));
                item.Start(_monitorCancellation.Token);
            });
        }

        private void Btn_RemoveMon_Click(object sender, EventArgs e)
        {
            var selectedItems = CLbx_Monitor.SelectedItems;

            if (CLbx_Monitor.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                    CLbx_Monitor.Items.Remove(selectedItems[i]);
            }
        }

        private void Btn_SaveRestart_Click(object sender, EventArgs e)
        {
           this.Hide();
           Task.Run(() => MainForm_FormClosed(null, null));
           Process.Start(Application.ExecutablePath);
        }

        private void Btn_ClearAllLogs_Click(object sender, EventArgs e)
        {
            Rtbx_EventLog.Clear();
            Rtbx_DebugLog.Clear();
        }
    }
}
