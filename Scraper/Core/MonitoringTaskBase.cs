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
        public enum FinalAction { PostToSlack, PostToDiscord };

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
                    case SearchMonitoringTask.FinalAction.PostToSlack:
                        foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                        {
                            SlackWebHook.PostMessage(product, slackUrl).ContinueWith(task =>
                            {
                                if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({product}) Sent To Slack");
                                if (task.IsFaulted) Logger.Instance.WriteErrorLog($"({product}) Slack Send Error");
                            }, token);
                        }

                        break;
                    case SearchMonitoringTask.FinalAction.PostToDiscord:
                        foreach (var discordUrl in AppSettings.Default.DiscordApiUrl)
                        {
                            DiscordWebhook.Send(discordUrl, product).ContinueWith(task =>
                            {
                                if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({product}) Sent To Discord");
                                if (task.IsFaulted) Logger.Instance.WriteErrorLog($"({product}) Discord Send Error");
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
