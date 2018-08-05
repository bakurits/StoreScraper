using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Endclothing;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class EndclothingTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            EndclothingScraper scraper = new EndclothingScraper();
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
            EndclothingScraper scraper = new EndclothingScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://www.endclothing.com/us/air-jordan-12-retro-gg-510815-100.html",
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
