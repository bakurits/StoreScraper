using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Bakurits.Baitme;
using StoreScraper.Bots.DavitBezhanishvili;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.DavitBezhanishvili
{
    [TestClass]
    public class AwLabScrapperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            var scraper = new AwLabScraper();
            var settings = new SearchSettingsBase()
            {
                KeyWords = "shoes"
            };

            scraper.FindItems(out var list, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(list);
        }

        [TestMethod]
        public void GetProductDetailsTest()
        {
            var scraper = new AwLabScraper();
            /*var testProduct = new Product(scraper, "Unknown",
                "https://en.aw-lab.com/shop/adidas-nizza-5895145?___SID=U",
                34.90,
                "https://en.aw-lab.com/shop/media/catalog/product/cache/3/small_image/190x/5e06319eda06f020e43594a9c230972d/5/8/5895145_0/adidas-nizza-20.jpg",
                "id", "€");
*/
            var testUrl = "https://en.aw-lab.com/shop/giacca-vans-boom-boom-9797389";

            var details = scraper.GetProductDetails(testUrl, CancellationToken.None);
            Helper.PrintGetDetailsResult(details.SizesList);
        }
    }
}
