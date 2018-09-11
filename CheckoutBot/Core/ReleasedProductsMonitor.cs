using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using CheckoutBot.CheckoutBots.FootSites;
using EO.Internal;
using EO.WebBrowser;
using StoreScraper.Models;

namespace CheckoutBot.Core
{
    public class ReleasedProductsMonitor
    {
        private static readonly ConcurrentDictionary<FootSitesBotBase, List<Product>> UpComingReleaseData =
            new ConcurrentDictionary<FootSitesBotBase, List<Product>>();

        public static List<Product> GetUpcomingReleases(FootSitesBotBase bot) => UpComingReleaseData.ContainsKey(bot) ? new List<Product>() : UpComingReleaseData[bot];
        public CancellationToken Token { private get; set; } = CancellationToken.None;
        public int MinutesToMonitor { private get; set; } = 10;
        
        public void ProductsMonitoringTask()
        {
            var timer = new System.Timers.Timer(TimeSpan.FromMinutes(MinutesToMonitor).Milliseconds);
            timer.Elapsed += UpdateUpcomingProductList;
            timer.Start();
        }

        private void UpdateUpcomingProductList(object sender, ElapsedEventArgs e)
        {
            foreach (var bot in AppData.AvailableBots)
            {
                UpComingReleaseData[bot] = bot.ScrapeReleasePage(Token);
            }
        }
    }
}