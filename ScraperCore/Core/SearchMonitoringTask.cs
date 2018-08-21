using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public class SearchMonitoringTask : MonitoringTaskBase
    {
        
        public List<ScraperBase> Stores { get; set; } = new List<ScraperBase>();
        public SearchSettingsBase SearchSettings { get; set; }

        /// <summary>
        /// TypeOfPayment of this is list of lists because separate list is for each store.
        /// </summary>
        public List<List<Product>> OldItems { get; set; } = new List<List<Product>>();

        public override void MonitorOnce(CancellationToken token)
        {
            for (int s = 0; s < Stores.Count; s++)
            {
                var oldSearch = OldItems[s];
                var store = Stores[s];
                Stores.AsParallel().ForAll(curSotre => { MonitorSingleStore(curSotre, oldSearch, token); });
            }

        }

        private void MonitorSingleStore(ScraperBase store, List<Product> oldSearch, CancellationToken token)
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
                oldSearch.Add(product);
                DoFinalActions(product.GetDetails(token), token);
            }
        }


        public override string ToString()
        {
            return $"{SearchSettings}, WebsitesCount: {Stores.Count}, FinalActions: {string.Join(",", FinalActions)}";
        }
    }
}
