﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperCore.Bots.Sticky_bit.EastBay_FootAction;
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
