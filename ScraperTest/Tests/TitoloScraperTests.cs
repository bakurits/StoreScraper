using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.titoloshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScraperTest;
using StoreScraper.Models;

namespace Tests
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