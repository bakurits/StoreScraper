using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Higuhigu.Consortium;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class ConsortiumScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            ConsortiumScraper scraper = new ConsortiumScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            ConsortiumScraper scraper = new ConsortiumScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "http://www.consortium.co.uk/adidas-originals-nmd-racer-primeknit-grey-one-grey-one-solar-pink.html",
                1,
                "whatever",
                "whatever",
                "GBP");

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
    }
}
