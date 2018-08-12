using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public abstract class MonitoringTaskBase
    {
        public enum FinalAction { PostToWebHook};

        public List<FinalAction> FinalActions { get; set; }
        public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        public abstract void MonitorOnce(CancellationToken token);

        public void Start(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        MonitorOnce(token);
                    }
                    catch
                    {
                        // ignored
                    }

                    Task.Delay(AppSettings.Default.MonitoringDelay, token).Wait(token);
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected void DoFinalActions(Product product, CancellationToken token)
        {
            foreach (var action in FinalActions)
            {
                switch (action)
                {
                    case SearchMonitoringTask.FinalAction.PostToWebHook:
                        foreach (var hook in AppSettings.Default.Webhooks)
                        {
                            hook.Poster.PostMessage(hook.WebHookUrl, product, TokenSource.Token).ContinueWith(task =>
                            {
                                if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({product}) Sent To Slack");
                                if (task.IsFaulted) Logger.Instance.WriteErrorLog($"({product}) Slack PostMessage Error");
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
