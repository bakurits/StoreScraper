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


        public List<ScraperBase> Stores { get; set; }
        public SearchSettingsBase SearchSettings { get; set; }

        /// <summary>
        /// Type of this is list of lists because separate list is for each store.
        /// </summary>
        public List<List<Product>> OldItems { get; set; }

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

        public void MonitorOnce(CancellationToken token)
        {
            List<Product> lst = null;


            for (int s = 0; s< Stores.Count; s++)
            {
                var oldSearch = OldItems[s];
                var store = Stores[s];
                Task.Run(() => MonitorSingleStore(store, oldSearch, token));
            }
           
        }

        private void MonitorSingleStore(ScraperBase store, List<Product> oldSearch, CancellationToken token)
        {
            List<Product> lst = null;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    store.FindItems(out lst, SearchSettings, token);
                    Logger.Instance.WriteErrorLog($"{store.WebsiteName} search success! found {lst.Count} products!!");
                    break;
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteErrorLog($"{store.WebsiteName} search failed rotating proxy.. \n Error msg: {e}");
                    if (i == 4) return;
                }
            }

            Logger.Instance.WriteVerboseLog($"({SearchSettings}) epoch completed");
            Debug.Assert(lst != null, nameof(lst) + " != null");
            foreach (var product in lst)
            {
                if (oldSearch.Contains(product)) continue;
                Logger.Instance.WriteVerboseLog($"New Item Appeared: {product}");
                oldSearch.Add(product);
                foreach (var action in FinalActions)
                {
                    switch (action)
                    {
                        case FinalAction.PostToSlack:
                            foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                            {
                                SlackWebHook.PostMessage(product, slackUrl).ContinueWith(task =>
                                {
                                    if (task.IsCompleted) Logger.Instance.WriteErrorLog($"({product}) Sent To Slack");
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
                }
            }
        }


        public override string ToString()
        {
            return $"{SearchSettings}, WebsitesCount: {Stores.Count}, FinalActions: {string.Join(",", FinalActions)}";
        }
    }
}
