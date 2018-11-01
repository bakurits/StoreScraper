using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Http.CookieCollecting;
using StoreScraper.Models;
using Timer = System.Timers.Timer;
#pragma warning disable 4014

namespace Scraper.Controls
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource _findTokenSource = new CancellationTokenSource();
        private BindingList<Product> _listOfProducts = new BindingList<Product>();
        private CancellationTokenSource _monitorCancellation = new CancellationTokenSource();
        private readonly Timer _timer = new Timer(5000);
        private bool _logchanged = false;

        public MainForm()
        {
            InitializeComponent();
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            DGrid_FoundProducts.DataSource = _listOfProducts;
            Clbx_Websites.Items.AddRange(Session.Current.AvailableScrapers.ToArray());
            PGrid_Settings.SelectedObject = AppSettings.Default;
            PGrid_Bot.SelectedObject = new SearchSettingsBase();
            RTbx_Proxies.Text = string.Join("\r\n", AppSettings.Default.Proxies);
            Logger.Instance.OnLogged += (message, color) => _logchanged = true;
            _timer.Elapsed += LogUpdaterElapsed;
            _timer.Start();
        }

        private void LogUpdaterElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!_logchanged) return;
            Rtbx_EventLog.Invoke((MethodInvoker) (() =>
            {
                Rtbx_EventLog.Clear();
                LogEntry[] localLogs = null;

                lock (Logger.Instance.Logs)
                {
                    localLogs = new LogEntry[Logger.Instance.Logs.Count];
                    Logger.Instance.Logs.CopyTo(localLogs);
                }
                foreach (var log in localLogs)
                {
                    Rtbx_EventLog.AppendText(log.Text, log.Color);
                }

                Rtbx_EventLog.SelectionStart = Rtbx_EventLog.Text.Length;
                Rtbx_EventLog.ScrollToCaret();
            }));

            _logchanged = false;
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
            foreach (var hook in AppSettings.Default.WebHooks)
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
                        if (Rtbx_DebugLog.Text.Length > Logger.MaxLogMesssages)
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

            Debug.Assert(grid != null, nameof(grid) + " != null");
            var product = grid.Rows[e.RowIndex].DataBoundItem as Product;

            if (product != null) Process.Start(product.Url);
        }

        private void Grids_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = sender as DataGridView;
            var cell = grid?.Rows[e.RowIndex].Cells[e.ColumnIndex];

            Debug.Assert(cell != null, nameof(cell) + " != null");
            cell.ToolTipText = "Double click to open in browser";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        { 
            Environment.Exit(Environment.ExitCode);
        }

        private void Cbx_ChooseStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            var bot = (sender as ComboBox)?.SelectedItem as ScraperBase;
            var sType = bot?.SearchSettingsType;
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
            Session.Current.SaveSession();
        }



        private void Btn_AddToMonitor_Click(object sender, EventArgs e)
        {
            var websites = new List<ScraperBase>();

            foreach (var website in Clbx_Websites.CheckedItems)
            {
                websites.Add((ScraperBase)website);
            }

            var monOptions = new MonitoringOptions()
            {
                WebHooks = new List<WebHook>(),
                Filter = (SearchSettingsBase) PGrid_Bot.SelectedObject,
            };

            AppSettings.Default.WebHooks.ForEach(hook => monOptions.WebHooks.Add(hook));

            if (!string.IsNullOrWhiteSpace(Tbx_CustomWebHook.Text))
            {
                monOptions.WebHooks.Add(new WebHook(){WebHookUrl = Tbx_CustomWebHook.Text});
            }

            var taskGroup = new SearchMonitoringTaskGroup()
            {
                Name = Tbx_TaskName.Text,
                Options = monOptions,
                WebsiteList = websites,
            };

            Session.Current.TaskManager.AddSearchTaskGroup(taskGroup);
        }


        private void Btn_RemoveMon_Click(object sender, EventArgs e)
        {
            var selectedItems = CLbx_Monitor.SelectedItems;

            if (CLbx_Monitor.SelectedIndex != -1)
            {
                for (int i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = selectedItems[i];
                    var group = (item as SearchMonitoringTaskGroup);

                    Session.Current.TaskManager.RemoveSearchTaskGroup(group);
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
            AppSettings.Default = new AppSettings {Proxies = proxies};
            AppSettings.Default.Save();
            Session.Current.SaveSession();
        }

        private void Btn_UrlMon_Click(object sender, EventArgs e)
        {
            UrlMonitoringTask task = new UrlMonitoringTask(Tbx_Url.Text);
            task.Start();
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

        private void btn_DeselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Clbx_Websites.Items.Count; i++)
            {
                Clbx_Websites.SetItemChecked(i, false);
            }
        }

        private void btn_SelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Clbx_Websites.Items.Count; i++)
            {
                Clbx_Websites.SetItemChecked(i, true);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Session.Current.TaskManager.MonTasksContainer = CLbx_Monitor;
        }

        private void Btn_Export_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                DefaultExt = "txt"
            };
            var result = dialog.ShowDialog();
            if(result != DialogResult.OK)return;
            
            var list = new List<ScraperBase>();

            foreach (var item in Clbx_Websites.CheckedItems)
            {
                list.Add((ScraperBase)item);
            }

            var file = dialog.OpenFile();

            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(string.Join(Environment.NewLine, list));
            }
        }
    }
}
