using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Models;
using EO.Internal;
using EO.WebBrowser;
using StoreScraper.Models;
using Timer = System.Timers.Timer;

namespace CheckoutBot.Core
{
    public class ReleasedProductsMonitor
    {
        public static ReleasedProductsMonitor Default { get; set; } = new ReleasedProductsMonitor();

        private readonly ConcurrentDictionary<FootSitesBotBase, List<FootsitesProduct>> _upComingReleaseData =
            new ConcurrentDictionary<FootSitesBotBase, List<FootsitesProduct>>();

        public List<FootsitesProduct> GetUpcomingReleases(FootSitesBotBase bot) => _upComingReleaseData.ContainsKey(bot) ? new List<FootsitesProduct>() : _upComingReleaseData[bot];
        public CancellationToken Token { private get; set; } = CancellationToken.None;
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int MinutesToMonitor { private get; set; } = 10;
        private readonly Timer _timer = new Timer(1000 * 60 * 10);
        
        public ReleasedProductsMonitor()
        {
            _timer.Elapsed += UpdateUpcomingProductList;
            _timer.Start();
            UpdateUpcomingProductList(null, null);
        }

        private void UpdateUpcomingProductList(object sender, ElapsedEventArgs e)
        {
            foreach (var bot in AppData.AvailableBots)
            {
                _upComingReleaseData[bot] = bot.ScrapeReleasePage(Token);
            }
        }


        public IEnumerable<FootsitesProduct> GetProductsList(FootSitesBotBase website)
        {
            return _upComingReleaseData[website];
        }
    }
}