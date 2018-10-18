using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public class MonitoringTaskManager
    {
        private const string UrlTasksFileName = "UrlTasks.json";
        private const string SearchTaskGroupsFileName = "SearchTaskGroups.json";

        static MonitoringTaskManager()
        {
            UrlTasksFilePath = Path.Combine(AppSettings.DataDir, UrlTasksFileName);
            SearchTaskGroupsFilePath = Path.Combine(AppSettings.DataDir, SearchTaskGroupsFileName);
        }

        public static MonitoringTaskManager Default { get; set; } = new MonitoringTaskManager();

        private static string UrlTasksFilePath { get; }
        private static string SearchTaskGroupsFilePath { get; }

        private Dictionary<ScraperBase, SearchMonitoringTask> SearchMonTasks { get; set; } = new Dictionary<ScraperBase, SearchMonitoringTask>();

        public List<UrlMonitoringTask> UrlMonTasks { get; set; } = new List<UrlMonitoringTask>();

        public CheckedListBox MonTasksContainer { get; set; }

        public async Task AddSearchTaskGroup(SearchMonitoringTaskGroup taskGroup)
        {
            var stores = taskGroup.WebsiteList;

            await Task.Run(() =>
            {


                StreamWriter errorLogTxtFile = null;
                StreamWriter verboseLogTxtFile = null;
                bool isNewWebsite = false;

                string workingWebsiteLst = $"THESE WEBSITES SUCCESSFULLY ADDED TO MONITOR: \n\n";
                string errorLog = $"ERROR WHILE ADDING THESE WEBSITES TO MONITOR: \n\n";
                Parallel.ForEach(stores, store =>
                {
                    if (!this.SearchMonTasks.TryGetValue(store, out var monTask))
                    {
                        monTask = new SearchMonitoringTask(taskGroup, store);


                        try
                        {
                            CancellationTokenSource tknSource = new CancellationTokenSource();
                            tknSource.CancelAfter(TimeSpan.FromSeconds(20));
                            ProductMonitoringManager.Default.RegisterMonitoringTaskAsync(store,  tknSource.Token).Wait();
                            ProductMonitoringManager.Default.AddNewProductHandler(store, monTask.HandleNewProduct);
                            SearchMonTasks.Add(store, monTask);
                            workingWebsiteLst += store.WebsiteName += Environment.NewLine;
                            isNewWebsite = true;
                        }
                        catch (Exception e)
                        {

                            lock (taskGroup.WebsiteList)
                            {
                                taskGroup.WebsiteList.Remove(store);
                            }

                            lock (monTask)
                            {
                                if (errorLogTxtFile == null)
                                {
                                    string filePath = Path.Combine("Logs",
                                        $"AddToMon ErrorLog ({DateTime.UtcNow:u})".EscapeFileName());
                                    errorLogTxtFile = File.CreateText(filePath);
                                }
                                errorLog += $"[{store.WebsiteName}] - error msg: {e.Message}";
                            }
                        }
                    }
                    else
                    {
                        lock (monTask.MonitoringOptions)
                        {
                            monTask.MonitoringOptions.Add(taskGroup.Options);
                        }
                    }
                });

                if(errorLogTxtFile != null)MessageBox.Show(errorLog, "Error occured while adding these websites in monitor!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                lock (taskGroup.WebsiteList)
                {
                    if (taskGroup.WebsiteList.Count > 0)
                    {
                        MessageBox.Show(workingWebsiteLst, "Adding process for these websites was successful!!!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MonTasksContainer.Invoke((MethodInvoker)(() => { MonTasksContainer.Items.Add(taskGroup); }));
                    } 
                }

                if (isNewWebsite)
                {
                    string verboseFilePath = Path.Combine("Logs", $"AddToMon VerboseLog ({DateTime.UtcNow:u})".EscapeFileName());
                    verboseLogTxtFile = File.CreateText(verboseFilePath);
                    verboseLogTxtFile.Write(workingWebsiteLst);
                    verboseLogTxtFile.Flush();

                    if (errorLogTxtFile != null)
                    {
                        errorLogTxtFile.Write(errorLog);
                        errorLogTxtFile.Flush();
                        errorLogTxtFile.Close();
                    }
                }

            });
        }

        public void RemoveSearchTaskGroup(SearchMonitoringTaskGroup taskGroup)
        {
            MonTasksContainer.Items.Remove(taskGroup);
            foreach (var scraper in taskGroup.WebsiteList)
            {
                var searchTask = SearchMonTasks[scraper];

                searchTask.MonitoringOptions.Remove(taskGroup.Options);

                if (searchTask.MonitoringOptions.Count == 0)
                {
                    searchTask.TokenSource.Cancel();
                    SearchMonTasks.Remove(scraper);
                }
            }
        }
    }
}
