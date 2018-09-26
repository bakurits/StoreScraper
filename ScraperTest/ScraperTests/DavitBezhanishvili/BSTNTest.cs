using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.BSTN;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class BSTNTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            BSTNScraper scraper = new BSTNScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
        [TestMethod]
        public void GetProductDetailsTest()
        {
            var scraper = new BSTNScraper();

            const string testUrl = "https://www.bstn.com/en/p/jordan-air-jordan-xxxii-low-basketball-aa1256-700-78736";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }

        [TestMethod]
        public void NewArrivalsTest()
        {
            var scraper = new BSTNScraper();

            scraper.ScrapeNewArrivalsPage(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

    }
}