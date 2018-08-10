using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Higuhigu.Ycmc;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class YcmcScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            YcmcScraper scraper = new YcmcScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            YcmcScraper scraper = new YcmcScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://www.ycmc.com/new-arrivals/adibreak-romper.html",
                1,
                "whatever",
                "whatever",
                "USD");

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
    }
}
