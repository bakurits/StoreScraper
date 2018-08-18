using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.YOOX;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Stycky_bit
{
    [TestClass]
    public class YOOXTest
    {
        [TestMethod]
        public void FindItems()
        {
            YOOXScraper scraper = new YOOXScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void GetProductDetails()
        {
            YOOXScraper scraper = new YOOXScraper();
            ProductDetails productDetails = scraper.GetProductDetails("https://www.yoox.com/us/12179224QO/item#dept=men&sts=SearchResult&cod10=12179224QO&sizeId=&sizeName=", CancellationToken.None);
            Console.WriteLine(productDetails);
        }
    }
}
