using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.titoloshop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScraperTest;

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
    }
}