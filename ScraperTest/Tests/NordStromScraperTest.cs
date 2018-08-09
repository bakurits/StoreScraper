using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Nordstrom;
using StoreScraper.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreScraper.Bots.BSTN;
using ScraperTest.Helpers;

namespace ScraperTest.Tests
{
    [TestClass]
    public class NordstromScraperTest
    {
        private NordstromScraper scraper = new NordstromScraper();
        public NordstromScraper Scraper { get => scraper; set => scraper = value; }

        [TestMethod]
        public void TestFind()
        {

        }

        [TestMethod]
        public void TestGetProductDetails()
        {
            var product = new Product()
            {
                Url = "https://shop.nordstrom.com/s/good-hyouman-lexi-new-day-tee/4983229?origin=keywordsearch-personalize...",
                ScrapedBy = Scraper
            };

            var details = product.GetDetails(CancellationToken.None);

            foreach (var size in details.SizesList)
            {
                Console.WriteLine(size.Key);

            }
    
        }
    }
}
