using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Baitme;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class BaitmeScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            BaitmeScraper scraper = new BaitmeScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jacket"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
