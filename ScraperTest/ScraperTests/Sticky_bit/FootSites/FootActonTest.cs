using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FootLocker_FootAction;
using ScraperTest.Helpers;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Sticky_bit.FootSites
{
    [TestClass]
    public class FootActonTest
    {
        [TestMethod]
        public void FindItems()
        {
            FootAPIBase.FootActionScraper scraper = new FootAPIBase.FootActionScraper();
            scraper.FindItems(out var lst, Helper.FootApiSearchSettingsSearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void GetProductDetails()
        {
            FootAPIBase.FootActionScraper scraper = new FootAPIBase.FootActionScraper();
            ProductDetails productDetails = scraper.GetProductDetails("https://www.footaction.com/product/nike-gx-pack-tank--mens/13177010.html?prValue=5145679", CancellationToken.None);
            Console.WriteLine(productDetails);
        }
    }
}
