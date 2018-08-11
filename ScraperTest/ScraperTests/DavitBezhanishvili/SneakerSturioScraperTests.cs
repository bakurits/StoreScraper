using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.DavitBezhanishvili.SneakerStudioScraper;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class SneakerSturioScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new SneakerStudioScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "adidas"
            };

            scraper.FindItems(out var list, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }
    }
}
