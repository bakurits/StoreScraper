using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace StoreScraper.Core
{
    public class SearchMonitoringTask : MonitoringTaskBase
    {
        
        public List<ScraperBase> Stores { get; set; } = new List<ScraperBase>();
        public SearchSettingsBase SearchSettings { get; set; }

        /// <summary>
        /// TypeOfPayment of this is list of lists because separate list is for each store.
        /// </summary>
        public List<HashSet<Product>> OldItems { get; set; } = new List<HashSet<Product>>();

        public override void MonitoringProcess(CancellationToken token)
        {
            var monTasks = new Task[Stores.Count];
            SearchSettings.RequiredScrappingLevel = ScrappingLevel.Url; // only url required in monitoring
            for (int s = 0; s < Stores.Count; s++)
            {
                var oldSearch = OldItems[s];
                var store = Stores[s];
                monTasks[s] = Task.Run(() => MonitorSingleStore(store, oldSearch, token), token);
            }

            Task.WaitAll(monTasks);
        }

        private void MonitorSingleStore(ScraperBase store, ISet<Product> oldSearch, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                List<Product> lst = null;

                try
                {
                    store.ScrapeItems(out lst, SearchSettings, token);
                    Logger.Instance.WriteVerboseLog($"{store.WebsiteName} search success! found {lst.Count} products!!");
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteErrorLog($"{store.WebsiteName} search failed!!\n Error msg: {e}");
                }

                Logger.Instance.WriteVerboseLog($"({SearchSettings}) epoch completed");
                Debug.Assert(lst != null, nameof(lst) + " != null");
                foreach (var product in lst)
                {
                    if (oldSearch.Contains(product)) continue;
                    Logger.Instance.WriteVerboseLog($"New Item Appeared: {product}");
                    var details = product.GetDetails(token);

                    if (!Utils.SatisfiesCriteria(details, SearchSettings)) continue;
                    oldSearch.Add(product);
                    DoFinalActions(details, token);
                }

                Task.Delay(AppSettings.Default.MonitoringDelay, token).Wait(token);
            }
        }


        public override string ToString()
        {
            return $"{SearchSettings}, WebsitesCount: {Stores.Count}, FinalActions: {string.Join(",", FinalActions)}";
        }
    }
}
