using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Sticky_bit.FootLocker_FootAction;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Sticky_bit.FootSites
{
    [TestClass]
    public class FootLockerTest
    {
        [TestMethod]
        public void FindItems()
        {
            FootAPIBase.FootLockerScraper scraper = new FootAPIBase.FootLockerScraper();
            scraper.FindItems(out var lst, Helper.FootApiSearchSettingsSearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void GetProductDetails()
        {
            FootAPIBase.FootLockerScraper scraper = new FootAPIBase.FootLockerScraper();
            ProductDetails productDetails = scraper.GetProductDetails("https://www.footlocker.com/product/Jordan%20Retro%201%20High%20OG%20-%20Men%27s/55088401.html", CancellationToken.None);
            Console.WriteLine(productDetails);
        }
    }
}
