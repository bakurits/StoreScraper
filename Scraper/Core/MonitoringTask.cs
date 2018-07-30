﻿using System;
using System.Collections.Generic;
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

                    Task.Delay(AppSettings.Default.MonitoringDelay, token).Wait();
                }
            });
        }

        public bool MonitorOnce(CancellationToken token)
        {
            List<Product> lst = null;

           
                try
                {
                    Bot.FindItems(out lst, SearchSettings, token);
                }
                catch
                {
                    //ignored
                }

            var result = false;
            foreach (var product in lst)
            {
                if (!OldItems.Contains(product))
                {
                    OldItems.Add(product);
                    foreach (var action in FinalActions)
                    {
                        if (action == FinalAction.PostToSlack)
                        {
                            foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                            {
                                try
                                {
                                    SlackWebHook.PostMessage(product, slackUrl);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        else if (action == FinalAction.PostToDiscord)
                        {
                            foreach (var discordUrl in AppSettings.Default.DiscordApiUrl)
                            {
                                try
                                {
                                    DiscordWebhook.Send(discordUrl, product);

                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                        result = true;
                    }
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
