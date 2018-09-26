using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public abstract class MonitoringTaskBase
    {
        public enum FinalAction { PostToWebHook};

        public List<FinalAction> FinalActions { get; set; }
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public abstract void MonitoringProcess(CancellationToken token);

        public void Start(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
               MonitoringProcess(token);
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected void DoFinalActions(ProductDetails productDetails, CancellationToken token)
        {
            productDetails.Validate();

            foreach (var action in FinalActions)
            {
                switch (action)
                {
                    case SearchMonitoringTask.FinalAction.PostToWebHook:
                        foreach (var hook in AppSettings.Default.Webhooks)
                        {
                            hook.Poster.PostMessage(hook.WebHookUrl, productDetails, TokenSource.Token).ContinueWith(task =>
                            {
                                if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({productDetails}) Sent To Slack");
                                if (task.IsFaulted) Logger.Instance.WriteErrorLog($"({productDetails}) Slack PostMessage Error");
                            }, token);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
