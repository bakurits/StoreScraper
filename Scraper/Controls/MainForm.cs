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
using StoreScraper.Browser;
using StoreScraper.Core;
using StoreScraper.Models;

namespace StoreScraper.Controls
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource _findTokenSource = new CancellationTokenSource();
        private BindingList<Product> _listOfProducts = new BindingList<Product>();
        private Logger _findProductsLogger = new Logger();
        private Thread _monitorThread;
        private CancellationTokenSource _monitorCancellation;

        public MainForm()
        {
            InitializeComponent();
            DGrid_FoundProducts.DataSource = _listOfProducts;
            Cbx_ChooseStore.Items.AddRange(AppSettings.Default.AvaibleBots.ToArray());
            PGrid_Settings.SelectedObject = AppSettings.Default;
            RTbx_Proxies.Text = string.Join("\r\n", AppSettings.Default.Proxies);
            _monitorThread = new Thread(Monitor);
            this.Shown += (sender, args) => _monitorThread.Start();
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
        }


        private void Monitor()
        {
           _monitorCancellation = new CancellationTokenSource();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true)
            {
                stopWatch.Restart();
                _monitorCancellation.Token.ThrowIfCancellationRequested();
                List<CartItem> cartItems = new List<CartItem>();
                List<CheckoutTask> settings = new List<CheckoutTask>();

                var checkeItems = new object[CLbx_Monitor.CheckedItems.Count];

                CLbx_Monitor.Invoke((MethodInvoker)(() => 
                    CLbx_Monitor.CheckedItems.CopyTo(checkeItems, 0)));
                


                foreach (var checkedItem in checkeItems)
                {
                    if (checkedItem is CartItem)
                    {
                        cartItems.Add(checkedItem as CartItem);
                    }
                    else if (checkedItem is CheckoutTask)
                    {
                        settings.Add(checkedItem as CheckoutTask);
                    }
                    else
                    {
                        (checkedItem as MonitoringTask)?.Do(_monitorCancellation.Token);
                    }
                }
                


                Thread.Sleep(Math.Max(AppSettings.Default.MonitoringDelay, 100));
                try
                {
                    l_MonInterval.Invoke(new Action(() =>
                        l_MonInterval.Text = $"Monitoring Interval: {stopWatch.Elapsed}"));
                }
                catch
                {
                    //ignored
                }

            }
          
        }

        private void btn_FindProducts_Click(object sender, EventArgs e)
        {
            _findTokenSource = new CancellationTokenSource();
            _listOfProducts.Clear();
            DGrid_FoundProducts.Refresh();
            btn_FindProducts.Enabled = false;
            label_FindingStatus.ForeColor = Color.SaddleBrown;
            label_FindingStatus.Text = @"Processing...";

            _findProductsLogger.OnLogged += InfoOnOnLogged;
            var bot = Cbx_ChooseStore.SelectedItem as ScraperBase;

            Task.Run
                (
                () =>
                {

                    bot.FindItems(out var products, PGrid_Bot.SelectedObject as SearchSettingsBase, _findTokenSource.Token, _findProductsLogger);
                    DGrid_FoundProducts.Invoke(new Action(() =>
                    {
                        lock (_listOfProducts)
                        {
                            products.ForEach(product => _listOfProducts.Add(product));
                            _findTokenSource.Token.ThrowIfCancellationRequested();
                        }
                    }));
                }
                ).ContinueWith(FindProductsTaskCompleted);

        }

        private void InfoOnOnLogged(object sender, string s)
        {
            Rtbx_EventLog.Invoke(new Action(() =>
                            Rtbx_EventLog.AppendText(s + "\r\n")
            ));
        }


        private void FindProductsTaskCompleted(Task task)
        {
            DGrid_FoundProducts.Invoke(new Action(() =>
            {
                if (task.IsFaulted)
                {
                    Rtbx_DebugLog.AppendText(task.Exception?.Message + "\r\n");
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
            if (grid.Columns[e.ColumnIndex].Name == "Price")
            {
                cell.Style.Format = "c";
            }
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
            AppSettings.Default.Proxies = RTbx_Proxies.Text.Remove('\r').Split('\n').ToList();
        }

        private void Btn_AddToMonitor_Click(object sender, EventArgs e)
        {
            var bot = (ScraperBase) Cbx_ChooseStore.SelectedValue;
            btn_FindProducts.PerformClick();
            var storeIndex = Cbx_ChooseStore.SelectedIndex;
            var searchOptions = (SearchSettingsBase)PGrid_Bot.SelectedObject;

            ActionChooser form = new ActionChooser();
            form.ShowDialog();
            var actions = new List<MonitoringTask.FinalAction>();

            if (form.DialogResult == DialogResult.OK)
            {
                actions.AddRange(form.GetFinalActions());
            }

            var item = new MonitoringTask()
            {
                SearchSettings = searchOptions,
                Bot = AppSettings.Default.AvaibleBots[storeIndex],
                Actions = actions,
                OldItems = _listOfProducts
            };

            CLbx_Monitor.Items.Add(item);
        }
    }
}
