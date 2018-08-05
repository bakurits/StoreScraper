using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.titoloshop;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass()]
    public class TitoloScraperTests
    {
        TitoloScraper scraper = new TitoloScraper();

        [TestMethod()]
        public void FindItemsTest()
        {
           scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);

           Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void TestGetProductDetails()
        {
            var product = new Product()
            {
                Url = "https://en.titoloshop.com/air-force-1-07-lv8-just-do-it-lntc",
                ScrapedBy = scraper
            };

            var details = product.GetDetails(CancellationToken.None);

            Console.WriteLine(string.Join(", ", details.SizesList));
        }
    }
}