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

        public abstract void StartMonitoring();

        public void Start()
        {
            Task.Factory.StartNew(StartMonitoring, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected void DoFinalActions(ProductDetails productDetails, CancellationToken token)
        {
            productDetails.Validate();

            foreach (var action in FinalActions)
            {
                switch (action)
                {
                    case FinalAction.PostToWebHook:
                        foreach (var hook in AppSettings.Default.Webhooks)
                        {
                            hook.Poster.PostMessage(hook.WebHookUrl, productDetails, TokenSource.Token).ContinueWith(task =>
                            {
                                if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({productDetails}) Sent To Slack");
                                if (task.IsFaulted)
                                {
                                    Logger.Instance.WriteErrorLog($"({productDetails}) Slack PostMessage Error. msg: {task.Exception?.InnerException?.Message}");
                                }
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
