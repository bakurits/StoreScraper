using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using CheckoutBot.CheckoutBots.FootSites;
using StoreScraper.Models;

namespace CheckoutBot.Core
{
    public static class Helper
    {
        private static Dictionary<FootSitesBotBase, List<Product>> UpComingReleaseData =
            new Dictionary<FootSitesBotBase, List<Product>>();

        public static List<Product> GetUpcomingReleases(FootSitesBotBase bot) => UpComingReleaseData.ContainsKey(bot) ? new List<Product>() : UpComingReleaseData[bot];


        public static void UpdateUpcomingProductList()
        {
            foreach (var bot in AppData.AvailableBots)
            {
                UpComingReleaseData[bot] = bot.ScrapeReleasePage(CancellationToken.None);
            }
        }
    }
}