using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StoreScraper.Helpers;
using StoreScraper.Models;

namespace StoreScraper.Core
{
    public class MonitoringTask : IMonitoringTask
    {
        public enum FinalAction { PostToSlack, PostToDiscord };


        public ScraperBase Bot { get; set; }
        public SearchSettingsBase SearchSettings { get; set; }
        public List<Product> OldItems { get; set; }
        public List<FinalAction> Actions;

        public bool Do(CancellationToken token)
        {
            Bot.FindItems(out var lst, SearchSettings, token, new Logger());
            var result = false;
            foreach (var product in lst)
            {
                if (!OldItems.Contains(product))
                {
                    OldItems.Add(product);
                    foreach (var action in Actions)
                    {
                        if (action == FinalAction.PostToSlack)
                        {
                            foreach (var slackUrl in AppSettings.Default.SlackApiUrl)
                            {
                                SlackWebHook.PostMessage(product, slackUrl);
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
