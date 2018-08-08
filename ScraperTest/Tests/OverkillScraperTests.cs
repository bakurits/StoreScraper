using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Overkill;
using StoreScraper.Models;
namespace ScraperTest.Tests
{
    [TestClass]
    public class OverkillScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            OverkillScraper scraper = new OverkillScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            OverkillScraper scraper = new OverkillScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://www.overkillshop.com/en/adidas-wmns-track-top-dh4196.html",
                1,
                "whatever",
                "whatever",
                "EUR");

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
    }
}
