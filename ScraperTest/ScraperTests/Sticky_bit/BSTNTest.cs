using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.BSTN;

namespace ScraperTest.ScraperTests.Stycky_bit
{
    [TestClass]
    public class BSTNTest
    {
        [TestMethod]
        public void BSTN()
        {
            BSTNScraper scraper = new BSTNScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}