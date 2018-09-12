using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.BSTN;

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

            const string testUrl = "https://www.bstn.com/en/p/adidas-continental-80-b41675-76624";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
        //    Debug.WriteLine(details.Currency);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}