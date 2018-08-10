using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mstanojevic.Nordstrom;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.GiorgiBaghdavadze
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
