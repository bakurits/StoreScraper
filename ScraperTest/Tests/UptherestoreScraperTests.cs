using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Uptherestore;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class UptherestoreScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            UptherestoreScraper scraper = new UptherestoreScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }



        [TestMethod()]
        public void GetProductDetailsTest()
        {
            UptherestoreScraper scraper = new UptherestoreScraper();
            Product curProduct = new Product(scraper,
                "Nike Air Jordan Wmns 1 Retro",
                "https://uptherestore.com/latest/og-cso-lx-black-white",
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
