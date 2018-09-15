using System.Collections.Generic;
using System.Threading;
using CheckoutBot.Models;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface IReleasePageScraper
    {
        /// <summary>
        /// Scrapes upcoming products from website.
        /// Called periodically to keep upcoming product information up to date
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        List<FootsitesProduct> ScrapeReleasePage(CancellationToken token);
    }
}