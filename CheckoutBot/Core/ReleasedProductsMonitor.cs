using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using CheckoutBot.CheckoutBots.FootSites;
using EO.Internal;
using EO.WebBrowser;
using StoreScraper.Models;
using Timer = System.Timers.Timer;

namespace CheckoutBot.Core
{
    public class ReleasedProductsMonitor
    {
        public static ReleasedProductsMonitor Default { get; set; }

        private readonly ConcurrentDictionary<FootSitesBotBase, List<Product>> UpComingReleaseData =
            new ConcurrentDictionary<FootSitesBotBase, List<Product>>();

        public List<Product> GetUpcomingReleases(FootSitesBotBase bot) => UpComingReleaseData.ContainsKey(bot) ? new List<Product>() : UpComingReleaseData[bot];
        public CancellationToken Token { private get; set; } = CancellationToken.None;
        public int MinutesToMonitor { private get; set; } = 10;
        private readonly Timer timer = new Timer();
        
        public ReleasedProductsMonitor()
        {
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