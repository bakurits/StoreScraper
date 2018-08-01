using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.ChampsSports_FootLocker_EastBay;

namespace ScraperTest.Tests
{
    [TestClass]
    public class FootScraperTest
    {

        [TestMethod]
        public void ChampsSportsScraper()
        {
            FootStoreScraper.ChampsSportsScraper scraper = new FootStoreScraper.ChampsSportsScraper();
            scraper.FindItems(out  var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void FootLocker()
        {
            FootStoreScraper.FootLockerScraper scraper = new FootStoreScraper.FootLockerScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void EastBay()
        {
            FootStoreScraper.EastBayScraper footStoreScraper = new FootStoreScraper.EastBayScraper();
            footStoreScraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }
    }
}