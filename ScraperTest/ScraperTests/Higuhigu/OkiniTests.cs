using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Higuhigu.Okini;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class OkiniTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            OkiniScraper scraper = new OkiniScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "sneaker"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            OkiniScraper scraper = new OkiniScraper();
            Product curProduct = new Product(scraper,
                "What",
                "https://row.oki-ni.com/nike-air-max-180-sneaker-in-black-wolf-grey-pink-blast-aq9974-001-68nikm2470blk",
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
