using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.BSTN;
using ScraperTest.Helpers;

namespace ScraperTest.Tests
{
    [TestClass]
    public class BSTNTest
    {
        [TestMethod]
        public void ChampsSportsScraper()
        {
            BSTNScraper scraper = new BSTNScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
