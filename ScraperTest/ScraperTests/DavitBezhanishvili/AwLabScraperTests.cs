using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.DavitBezhanishvili.AwLab;
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
       
            var testUrl = "https://en.aw-lab.com/shop/men/sale/cappello-under-armour-ua-microthread-twist-renegade-9895226";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}
