using System.Collections.Generic;
using System.Threading;
using StoreScraper.Models;

namespace CheckoutBot.Interfaces
{
    public interface IReleasePageScraper
    {
        List<Product> ScrapeReleasePage(CancellationToken token);
    }
}