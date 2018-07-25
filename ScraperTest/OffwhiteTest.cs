using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Models;
using StoreScraper.Scrapers.OffWhite;

namespace ScraperTest
{
    [TestClass]
    public class OffWhiteTest
    {
        public OffWhiteScraper Scraper = new OffWhiteScraper();

        [TestMethod]
        public void TestFind()
        {
            var searchSettings = Helper.SearchSettings;
            Scraper.FindItems(out var listOfProducts, searchSettings, CancellationToken.None, new Logger());
            Helper.PrintTestReuslts(listOfProducts);
        }
    }
}
