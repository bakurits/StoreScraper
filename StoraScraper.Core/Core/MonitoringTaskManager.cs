using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    [JsonObject]
    public class MonitoringTaskManager
    {
        private const string UrlTasksFileName = "UrlTasks.json";
        private const string SearchTaskGroupsFileName = "SearchTaskGroups.json";

        static MonitoringTaskManager()
        {
            UrlTasksFilePath = Path.Combine(AppSettings.DataDir, UrlTasksFileName);
            SearchTaskGroupsFilePath = Path.Combine(AppSettings.DataDir, SearchTaskGroupsFileName);
        }


        private static string UrlTasksFilePath { get; }
        private static string SearchTaskGroupsFilePath { get; }

        [JsonIgnore]
        private Dictionary<ScraperBase, SearchMonitoringTask> SearchMonTasks { get; set; } = new Dictionary<ScraperBase, SearchMonitoringTask>();

        [JsonProperty]
        public List<SearchMonitoringTask> SearchMonTaskList 
        { 
            get => SearchMonTasks.ToList().Select(elem => elem.Value).ToList(); 
            set => SearchMonTasks = value.ToDictionary(elem => elem.Store, elem => elem);
        }

        public List<UrlMonitoringTask> UrlMonTasks { get; set; } = new List<UrlMonitoringTask>();

        [JsonIgnore]
        public CheckedListBox MonTasksContainer { get; set; }

        public async Task AddSearchTaskGroup(SearchMonitoringTaskGroup taskGroup)
        {
            var stores = taskGroup.WebsiteList;

            await Task.Run(() =>
            {


                StreamWriter errorLogTxtFile = null;
                bool isNewWebsite = false;

                string workingWebsiteLog = $"THESE WEBSITES SUCCESSFULLY ADDED TO MONITOR: \n\n";
                List<ScraperBase> workingWebsiteList = new List<ScraperBase>();
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
                            Session.Current.MonitoringManager.RegisterMonitoringTaskAsync(store,  tknSource.Token).Wait();
                            Session.Current.MonitoringManager.AddNewProductHandler(store, monTask.HandleNewProduct);
                            SearchMonTasks.Add(store, monTask);
                            workingWebsiteList.Add(store);
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


                workingWebsiteLog += string.Join(Environment.NewLine, workingWebsiteList);
                lock (taskGroup.WebsiteList)
                {
                    if (taskGroup.WebsiteList.Count > 0)
                    {
                        MessageBox.Show(workingWebsiteLog, "Adding process for these websites was successful!!!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MonTasksContainer.Invoke((MethodInvoker)(() => { MonTasksContainer.Items.Add(taskGroup); }));
                    } 
                }

                if (isNewWebsite)
                {
                    string verboseFilePath = Path.Combine("Logs", $"AddToMon VerboseLog ({DateTime.UtcNow:u})".EscapeFileName());
                    var verboseLogTxtFile = File.CreateText(verboseFilePath);
                    verboseLogTxtFile.Write(workingWebsiteLog);
                    verboseLogTxtFile.Flush();
                    verboseLogTxtFile.Close();

                    if (errorLogTxtFile != null)
                    {
                        errorLogTxtFile.Write(errorLog);
                        errorLogTxtFile.Flush();
                        errorLogTxtFile.Close();
                    }
                }

                workingWebsiteList.ForEach(store => Session.Current.MonitoringManager.StartMonitoringTask(store));

            });
        }

        public void RemoveSearchTaskGroup(SearchMonitoringTaskGroup taskGroup)
        {
            MonTasksContainer.Items.Remove(taskGroup);
            foreach (var scraper in taskGroup.WebsiteList)
            {
                var searchTask = SearchMonTasks[scraper];

                searchTask.MonitoringOptions.Remove(taskGroup.Options);

                if (searchTask.MonitoringOptions.Count != 0) continue;
                searchTask.TokenSource.Cancel();
                SearchMonTasks.Remove(scraper);
            }
        }
    }
}
