using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Sticky_bit.ChampsSports_EastBay;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Sticky_bit.FootSites
{
    [TestClass]
    public class EastBayTest
    {

        [TestMethod]
        public void FindItems()
        {
            FootSimpleBase.EastBayScraper scraper = new FootSimpleBase.EastBayScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        [TestMethod]
        public void GetProductDetails()
        {
            FootSimpleBase.EastBayScraper scraper = new FootSimpleBase.EastBayScraper();
            ProductDetails productDetails = scraper.GetProductDetails("https://www.eastbay.com/product/model:286098/sku:A111794/easton-ghost-x-bbcor-baseball-bat-mens/white/gold/", CancellationToken.None);
            Console.Write(productDetails); 
        }
    }
}
