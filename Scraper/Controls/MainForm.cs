using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.CookieCollecting;
using StoreScraper.Models;

namespace Scraper.Controls
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
            Clbx_Websites.Items.AddRange(AppSettings.Default.AvailableScrapers.ToArray());
            PGrid_Settings.SelectedObject = AppSettings.Default;
            PGrid_Bot.SelectedObject = new SearchSettingsBase();
            RTbx_Proxies.Text = string.Join("\r\n", AppSettings.Default.Proxies);
            Logger.Instance.OnLogged += (message, color) =>
            {
                Rtbx_EventLog.Invoke((MethodInvoker)delegate
                {
                    if (Rtbx_EventLog.Text.Length > Logger.MaxLogBytes)
                    {
                        Rtbx_EventLog.SaveFile($"Logs\\EventLog{DateTime.Now.ToFileTime()}");
                        Rtbx_EventLog.Clear();
                    }
                });

                Rtbx_EventLog.AppendText(message, color);
            };
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

            var scrapers = Clbx_Websites.CheckedItems;

            var tasks = new List<Task>();

            foreach (var obj in scrapers)
            {
                var scraper = (ScraperBase) obj;
                tasks.Add(Task.Run(() => FindAction(scraper)));
            }

            Task.WhenAll(tasks).ContinueWith(FindProductsTaskCompleted);
        }

        private void PostProduct(ProductDetails productDetails)
        {
            foreach (var hook in AppSettings.Default.Webhooks)
            {
                hook.Poster.PostMessage(hook.WebHookUrl, productDetails, _findTokenSource.Token);
            }
        }

        private void FindAction(ScraperBase scraper)
        {
            var searchOptions = (SearchSettingsBase) PGrid_Bot.SelectedObject;
            var convertedFilter = searchOptions;
            if (searchOptions.GetType() != scraper.SearchSettingsType)
            {
                convertedFilter = SearchSettingsBase.ConvertToChild(searchOptions, scraper.SearchSettingsType);
            }
            
            scraper.ScrapeItems(out var products, convertedFilter, _findTokenSource.Token);
            if (AppSettings.Default.PostStartMessage)
            {
                products.AsParallel().ForAll( product =>
                {
                    var pDetails = product.GetDetails(_findTokenSource.Token);
                    pDetails.Validate();
                    PostProduct(pDetails);
                });
            }


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

                    Rtbx_DebugLog.Invoke((MethodInvoker) delegate
                    {
                        if (Rtbx_DebugLog.Text.Length > Logger.MaxLogBytes)
                        {
                            Rtbx_DebugLog.Clear();
                        }
                    });

                    Rtbx_DebugLog.AppendText(task.Exception?.StackTrace + Environment.NewLine);
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
            CookieCollector.Default.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void Cbx_ChooseStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bot = (sender as ComboBox).SelectedItem as ScraperBase;
            var sType = bot.SearchSettingsType;
            PGrid_Bot.SelectedObject = Activator.CreateInstance(sType);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void Btn_Save_Click(object sender, EventArgs e)
        {
            AppSettings.Default.Proxies = RTbx_Proxies.Text.Replace("\r", "").Split('\n').ToList();
            if (AppSettings.Default.Proxies.Count == 1 && AppSettings.Default.Proxies.FirstOrDefault() == "")
            {
                AppSettings.Default.Proxies = new List<string>();
            }


            AppSettings.Default.Proxies.ConvertAll<string>(proxy =>
            {
                if (proxy.StartsWith("http://")) return proxy;
                if (proxy.StartsWith("https://")) return proxy.Replace("https://", "http://");

                return "http://" + proxy;
            });

            AppSettings.Default.Save();
        }



        private void Btn_AddToMonitor_Click(object sender, EventArgs e)
        {
            
            List<ScraperBase> stores = new List<ScraperBase>();

            foreach (var obj in Clbx_Websites.CheckedItems)
            {
                var store = (ScraperBase) obj;
                stores.Add(store);
            }

            MessageBox.Show(@"searching filter is now adding to monitor list
                              That may take several mins
                              You can continue using scraper while filter is adding..");


            AddToMonitorTask(stores).ContinueWith(task =>
            {
                MessageBox.Show("Search filter was succesfully added to monitoring list");
            });
        }

        public async Task AddToMonitorTask(List<ScraperBase> stores)
        {
            var searchOptions = (SearchSettingsBase)PGrid_Bot.SelectedObject;

            var monTask = new SearchMonitoringTask()
            {
                SearchSettings = searchOptions,
                FinalActions = new List<MonitoringTaskBase.FinalAction>() { MonitoringTaskBase.FinalAction.PostToWebHook }
            };

            await Task.Run(() =>
            {
                StreamWriter logTextFile = null;
                string log = $"Error while Adding There Websites to monitor: \n\n";
                foreach (var store in stores)
                {
                
                    monTask.Stores.Add(store);

                    var convertedFilter = searchOptions;
                    if (searchOptions.GetType() != store.SearchSettingsType &&
                        !searchOptions.GetType().IsSubclassOf(store.SearchSettingsType))
                    {
                        convertedFilter = SearchSettingsBase.ConvertToChild(searchOptions, store.SearchSettingsType);
                    }

                    try
                    {
                        store.ScrapeItems(out var curProductsList, convertedFilter, _findTokenSource.Token);
                        HashSet<Product> set = new HashSet<Product>(curProductsList);
                        monTask.OldItems.Add(set);
                    }
                    catch
                    {
                        MessageBox.Show($"Error Occured while trying to obtain current products with specified search criteria on {store.WebsiteName}");
                        if (logTextFile == null)
                        {
                            string filePath = Path.Combine("Logs",$"AddToMon ErrorLog ({DateTime.UtcNow:u})".EscapeFileName());
                            logTextFile = File.CreateText(filePath);
                        }

                        log += store.WebsiteName + Environment.NewLine;
                    }
                }

                if (logTextFile != null)
                {
                    logTextFile.Write(log);
                    logTextFile.Flush();
                    logTextFile.Close();
                }


                monTask.Stores.ForEach(store => store.Active = true);
                CLbx_Monitor.Invoke(new Action(() => CLbx_Monitor.Items.Add(monTask, true)));
                monTask.Start(monTask.TokenSource.Token);
            });
        }

        private void Btn_RemoveMon_Click(object sender, EventArgs e)
        {
            var selectedItems = CLbx_Monitor.SelectedItems;

            if (CLbx_Monitor.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = selectedItems[i];
                    (item as MonitoringTaskBase).TokenSource.Cancel();
                    CLbx_Monitor.Items.Remove(selectedItems[i]);
                }
                    
            }
        }

        private void Btn_SaveRestart_Click(object sender, EventArgs e)
        {
            Btn_Save_Click(null, null);
            this.Hide();
            Process.Start(Application.ExecutablePath);
            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }

        private void Btn_ClearAllLogs_Click(object sender, EventArgs e)
        {
            Rtbx_EventLog.Clear();
            Rtbx_DebugLog.Clear();
        }

        private bool IsSameOrSubclass(Type @base, Type descendant)
        {
            return descendant.IsSubclassOf(@base)
                   || descendant == @base;
        }

        private void Clbx_Websites_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                CLbx_Monitor.BeginInvoke((MethodInvoker) delegate
                {
                
                    if (Clbx_Websites.CheckedIndices.Count == 1)
                    {
                        var scraper = (Clbx_Websites.CheckedItems[0] as ScraperBase);
                        if (!IsSameOrSubclass(scraper.GetType(), PGrid_Bot.SelectedObject.GetType()))
                        {
                            PGrid_Bot.SelectedObject = SearchSettingsBase.ConvertToChild((SearchSettingsBase)PGrid_Bot.SelectedObject, scraper.SearchSettingsType);
                        }
                    }
                });
            });
        }

        private void Btn_Reset_Click(object sender, EventArgs e)
        {
            var proxies = AppSettings.Default.Proxies;
            AppSettings.Default = new AppSettings();
            AppSettings.Default.Proxies = proxies;
            AppSettings.Default.Save();
        }

        private void Btn_UrlMon_Click(object sender, EventArgs e)
        {
            UrlMonitoringTask task = new UrlMonitoringTask(Tbx_Url.Text);
            task.Start(task.TokenSource.Token);
            CLbx_Monitor.Items.Add(task);
        }

        private void Clbx_Websites_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        private void btn_Keywords_Click(object sender, EventArgs e)
        {
            var settings = (SearchSettingsBase) PGrid_Bot.SelectedObject;
            KeywordInputForm form = new KeywordInputForm {Tbx_Input = {Text = settings.KeyWords}};
            form.ShowDialog();
            settings.KeyWords = form.ResultText;
        }

        private void btn_NegKeywords_Click(object sender, EventArgs e)
        {
            var settings = (SearchSettingsBase) PGrid_Bot.SelectedObject;
            KeywordInputForm form = new KeywordInputForm {Tbx_Input = {Text = settings.NegKeyWords}};
            form.ShowDialog();
            settings.NegKeyWords = form.ResultText;
        }
    }
}
