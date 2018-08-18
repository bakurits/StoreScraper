using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Sticky_bit.ChampsSports_FootLocker;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Sticky_bit.FootSites
{
    [TestClass]
    class ChampsSportsTest
    {
        [TestMethod]
        public void FindItems()
        {
            FootSimpleBase.ChampsSportsScraper scraper = new FootSimpleBase.ChampsSportsScraper();
            scraper.FindItems(out var lst, Helper.SearchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        public void GetProductDetails()
        {
            FootSimpleBase.ChampsSportsScraper scraper = new FootSimpleBase.ChampsSportsScraper();
            ProductDetails productDetails = scraper.GetProductDetails("https://www.champssports.com/product/model:258371/sku:B76079/adidas-originals-nmd-r1-mens/grey/white/#sku-B76079", CancellationToken.None);
            Console.WriteLine(productDetails);
        }
    }
}
