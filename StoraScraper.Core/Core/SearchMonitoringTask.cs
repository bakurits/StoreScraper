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
        public int GourpId { get; }
        public ScraperBase Store { get; protected set; } 
        public List<MonitoringOptions> MonitoringOptions { get; private set; }


        public SearchMonitoringTask(SearchMonitoringTaskGroup ownerGroup, ScraperBase store)
        {
            this.GourpId = ownerGroup.Id;
            this.Store = store;
            this.MonitoringOptions = new List<MonitoringOptions>();
        }

        public void HandleNewProduct(Product product)
        {
            MonitoringOptions.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism).ForAll(option =>
            {
                var tknSource = new CancellationTokenSource();
                tknSource.CancelAfter(TimeSpan.FromSeconds(20));
                var details = product.GetDetails(tknSource.Token);
                if (!Utils.SatisfiesCriteria(details, option.Filter)) return;
                foreach (var hook in option.WebHooks)
                {
                    hook.Poster.PostMessage(hook.WebHookUrl, details, tknSource.Token);
                } 
            });
        }
    }
}
