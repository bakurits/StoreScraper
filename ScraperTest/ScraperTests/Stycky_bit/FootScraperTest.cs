using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker_EastBay_FootAction;

namespace ScraperTest.ScraperTests.Stycky_bit
{
    [TestClass]
    public class FootScraperTest
    {
        [TestMethod]
        public void ChampsSportsScraper()
        {
            FootSimpleBase.ChampsSportsScraper scraper = new FootSimpleBase.ChampsSportsScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void EastBay()
        {
            FootSimpleBase.EastBayScraper scraper = new FootSimpleBase.EastBayScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void FootLocker()
        {
            FootAPIBase.FootLockerScraper scraper = new FootAPIBase.FootLockerScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void FootAction()
        {
            FootAPIBase.FootActionScraper scraper = new FootAPIBase.FootActionScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}