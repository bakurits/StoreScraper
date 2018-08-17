using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperCore.Bots.Sticky_bit.EastBay_FootAction;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Bots.Sticky_bit;

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
        public void FootLockerGetProducts()
        {
            FootAPIBase.FootLockerScraper scraper = new FootAPIBase.FootLockerScraper();
            scraper.FindItems(out var lst, Helper.FootApiSearchSettingsSearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void FootActionGetProducts()
        {
            FootAPIBase.FootActionScraper scraper = new FootAPIBase.FootActionScraper();
            scraper.FindItems(out var lst, Helper.FootApiSearchSettingsSearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void FootLockerGetProductDetails()
        {
            string url = "https://www.footlocker.com/product/nike-kobe-ad-nxt-360--mens/087102.html";
            FootAPIBase.FootLockerScraper scraper = new FootAPIBase.FootLockerScraper();
            Console.WriteLine(scraper.GetProductDetails(url, CancellationToken.None));
        }

        [TestMethod]
        public void FootActionGetProductDetails()
        {
            string url = "https://www.footaction.com/product/nike-gx-pack-tank--mens/13177010.html?prValue=5145679";
            FootAPIBase.FootActionScraper scraper = new FootAPIBase.FootActionScraper();
            Console.WriteLine(scraper.GetProductDetails(url, CancellationToken.None));
        }
    }
}