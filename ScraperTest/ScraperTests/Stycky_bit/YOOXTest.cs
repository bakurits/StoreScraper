using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.YOOX;

namespace ScraperTest.ScraperTests.Stycky_bit
{
    [TestClass]
    public class YOOXTest
    {
        [TestMethod]
        public void YOOX()
        {
            YOOXScraper scraper = new YOOXScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
