using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public abstract class MonitoringTaskBase
    {
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
       

        protected void SendNotification(ProductDetails productDetails, List<WebHook> webHooks)
        {
            productDetails.Validate();

            foreach (var hook in webHooks)
            {
                hook.Poster.PostMessage(hook.WebHookUrl, productDetails, TokenSource.Token).ContinueWith(task =>
                {
                    if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({productDetails}) Sent To Slack");
                    if (task.IsFaulted)
                    {
                        Logger.Instance.WriteErrorLog($"({productDetails}) WebHook PostMessage Error. msg: {task.Exception?.InnerException?.Message}");
                    }
                }, TokenSource.Token);
            }
        }
    }
}
