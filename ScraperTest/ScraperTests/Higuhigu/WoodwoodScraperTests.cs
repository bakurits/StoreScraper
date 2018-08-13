using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Higuhigu.Woodwood;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class WoodwoodScraperTests
    {

        [TestMethod]
        public void FindItemsTest()
        {
            WoodwoodScraper scraper = new WoodwoodScraper();
            WoodwoodSearchSettings settings = new WoodwoodSearchSettings()
            {
                KeyWords = "jordan",
                Gender = WoodwoodSearchSettings.GenderEnum.Man
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            WoodwoodScraper scraper = new WoodwoodScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://www.woodwood.com/commodity/5598-double-a-joel-jacket",
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
