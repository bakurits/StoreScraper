using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public class MonitoringTask
    {
        public enum FinalAction { PostToSlack, PostToDiscord };


        public ScraperBase Bot { get; set; }
        public SearchSettingsBase SearchSettings { get; set; }
        public List<Product> OldItems { get; set; }
        public List<FinalAction> FinalActions { get; set; }


        public void Start(CancellationToken token)
        {
            Task.Run(() =>
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
            }, token);
        }

        public bool MonitorOnce(CancellationToken token)
        {
            List<Product> lst = null;


            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Bot.FindItems(out lst, SearchSettings, token);
                    Logger.Instance.WriteErrorLog($"{Bot.WebsiteName} search success! found {lst.Count} products!!");
                    break;
                }
                catch
                {
                    Logger.Instance.WriteErrorLog($"{Bot.WebsiteName} search failed rotating proxy..");
                    if (i == 4) return false;
                }
            }

            

            var result = false;
            Logger.Instance.WriteVerboseLog($"({SearchSettings}) epoch completed");
            Debug.Assert(lst != null, nameof(lst) + " != null");
            foreach (var product in lst)
            {
                if (OldItems.Contains(product)) continue;
                Logger.Instance.WriteVerboseLog($"New Item Appeared: {product}");
                OldItems.Add(product);
                foreach (var action in FinalActions)
                {
                    switch (action)
                    {
                        case FinalAction.PostToSlack:
                            foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                            {
                                SlackWebHook.PostMessage(product, slackUrl).ContinueWith(task =>
                                {                               
                                    if(task.IsCompleted) Logger.Instance.WriteErrorLog($"({product}) Sent To Slack");
                                    if (task.IsFaulted) Logger.Instance.WriteErrorLog($"({product}) Slack Send Error");
                                }, token);
                            }

                            break;
                        case FinalAction.PostToDiscord:
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

                    result = true;
                }
            }
            return result;
        }


        public override string ToString()
        {
            return $"{SearchSettings}, ScrapedBy - {Bot}, FinalActions: {string.Join(",", FinalActions)}";
        }
    }
}
