using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.DavitBezhanishvili.AwLab;
using StoreScraper.Bots.Jordan.Ruvilla;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class RuvillaScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new RuvillaScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "shoes"
            };

            scraper.FindItems(out var list, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }

        [TestMethod]
        public void NewArrivalsTest()
        {
            var scraper = new RuvillaScraper();
           
            scraper.ScrapeNewArrivalsPage(out var list, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }


        [TestMethod]
        public void GetProductDetailsTest()
        {
            var scraper = new RuvillaScraper();

            var testUrl = "https://www.ruvilla.com/sale/men/footwear/new-balance-498-ml498kbb.html";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}
