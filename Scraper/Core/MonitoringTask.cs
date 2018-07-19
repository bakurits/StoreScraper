using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckOutBot.Helpers;
using CheckOutBot.Interfaces;
using CheckOutBot.Models;

namespace CheckOutBot.Core
{
    public class MonitoringTask : IMonitoringTask
    {
        public enum FinalAction { PostToSlack, PostToDiscord };


        public ScraperBase Bot { get; set; }
        public object SearchSettings { get; set; }
        public IEnumerable<Product> OldItems { get; set; }
        public IEnumerable<FinalAction> Actions;

        public bool Do(CancellationToken token)
        {
            Bot.FindItems(out var lst, SearchSettings, token, new Logger());
            var result = false;
            foreach (var product in lst)
            {
                if (!OldItems.Contains(product))
                {
                    OldItems.Append(product);
                    foreach (var action in Actions)
                    {
                        if (action == FinalAction.PostToSlack)
                        {
                            foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                            {
                                SlackWebHook.PostMessageAsync($"Product: *{product.Name}* Appeared!! Url : {product.Url}", slackUrl);
                            }
                        }
                        else if (action == FinalAction.PostToDiscord)
                        {
                            foreach (var discordUrl in AppSettings.Default.DiscordApiUrl)
                            {
                                DiscordWebhook.Send(discordUrl, $"Product: *{product.Name}* Appeared!! Url : {product.Url}");
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
            return $"{SearchSettings}, Store - {Bot}, Actions: {string.Join(",", Actions)}";
        }
    }
}
