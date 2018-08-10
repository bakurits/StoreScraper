using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Higuhigu._43Einhalb;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class EinhalbScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            EinhalbScraper scraper = new EinhalbScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan shoes"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }


        [TestMethod()]
        public void GetProductDetailsTest()
        {
            EinhalbScraper scraper = new EinhalbScraper();
            Product curProduct = new Product(scraper,
                "Nike Air Jordan Wmns 1 Retro",
                "https://www.43einhalb.com/en/nike-air-jordan-1-retro-high-premium-white-221783",
                1,
                "whatever",
                "whatever",
                "EUR");

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }

    }
}
