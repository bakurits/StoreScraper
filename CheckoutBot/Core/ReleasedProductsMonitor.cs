using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Models;
using Timer = System.Timers.Timer;

namespace CheckoutBot.Core
{
    public class ReleasedProductsMonitor
    {
        public static ReleasedProductsMonitor Default { get; set; }

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
            AppData.AvailableBots.AsParallel().ForAll(bot =>
            {
                var prods = bot.ScrapeReleasePage(Token);
                prods.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
                _upComingReleaseData[bot] = prods;
            });
        }

        public FootsitesProduct FindProductByUrl(string url)
        {
            foreach (var releaseGroup in _upComingReleaseData)
            {
                var result = releaseGroup.Value.Find(prod => prod.Url == url);
                if (result != null) return result;
            }

            return null;
        }


        public IEnumerable<FootsitesProduct> GetProductsList(FootSitesBotBase website)
        {
            return _upComingReleaseData[website];
        }
    }
}