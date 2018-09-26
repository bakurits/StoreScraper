using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FootLocker_FootAction;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
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
