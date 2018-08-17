using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.GiorgiBaghdavadze.Nordstrom;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.GiorgiBaghdavadze
{
    [TestClass]
    public class NordstromScraperTest
    {
        public NordstromScraper Scraper { get; set; } = new NordstromScraper();

        [TestMethod]
        public void TestFind()
        {
            Scraper.FindItems(out var lst, Helpers.Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
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
