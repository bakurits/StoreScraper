using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Bakurits.Baitme;
using StoreScraper.Bots.DavitBezhanishvili;
using StoreScraper.Bots.DavitBezhanishvili.AwLab;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class AwLabScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new AwLabScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "shoes"
            };
            
            scraper.FindItems(out var list, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }

        [TestMethod]
        public void GetProductDetailsTest()
        {
            var scraper = new AwLabScraper();
       
            var testUrl = "https://en.aw-lab.com/shop/women/sales/diadora-game-l-low-5041641";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Debug.WriteLine(details.Currency);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}
