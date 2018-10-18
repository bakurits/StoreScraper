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
            this.MonitoringOptions.Add(ownerGroup.Options);
        }

        public void HandleNewProduct(Product product)
        {
            Logger.Instance.WriteVerboseLog($"(NewProductHandler) Executing new product handler for {product}. {MonitoringOptions.Count} options attached");

            foreach (var option in MonitoringOptions)
            {
                Logger.Instance.WriteVerboseLog($"(NewProductHandler) Handling Monitoring Option {option.Filter}. {option.WebHooks.Count} Webhooks attached");
                var tknSource = new CancellationTokenSource();
                var token = tknSource.Token;
                tknSource.CancelAfter(TimeSpan.FromSeconds(20));


                Task.Factory.StartNew(() =>
                {
                    var details = product.GetDetails(token);
                    Logger.Instance.WriteVerboseLog(
                        $"(NewProductHandler) successfully got product details [{details.Url}]");
                    if (!Utils.SatisfiesCriteria(details, option.Filter)) return;
                    foreach (var hook in option.WebHooks)
                    {
                        hook.Poster.PostMessage(hook.WebHookUrl, details, token);
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }
    }
}
