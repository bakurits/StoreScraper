using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Models;
using static StoreScraper.Scrapers.ChampsSports_FootLocker_EastBay.FootStoreScraper;

namespace ScraperTest.Tests
{
    [TestClass]
    public class FootScraperTest
    {

        [TestMethod]
        public void ChampsSportsScraper()
        {
            ChampsSportsScraper scraper = new ChampsSportsScraper();
            scraper.FindItems(out  var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void FootLocker()
        {
            FootLockerScraper scraper = new FootLockerScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void EastBay()
        {
            EastBayScraper footStoreScraper = new EastBayScraper();
            footStoreScraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void ChampsSportsGetDetails()
        {
            string url =
                "https://www.champssports.com/product/model:299420/sku:A5905010/nike-jdi-club-pullover-hoodie-mens/";

            Product p = new Product(new ChampsSportsScraper(), "chudo", url, 30, null, url);
            var details = p.GetDetails(CancellationToken.None);
            
            Debug.WriteLine(string.Join(", ",details.SizesList));
        }
    }
}