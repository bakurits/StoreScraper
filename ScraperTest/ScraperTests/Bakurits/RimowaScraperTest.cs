using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Bakurits.Rimowa;
using StoreScraper.Bots.Bakurits.Shelflife;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class RimowaScraperTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            RimowaScraper scraper = new RimowaScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "luggage"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new ShelflifeScraper(), "INCOTEX Slim - Fit Pleated Brushed Stretch - Cotton Trousers",
                "https://www.shelflife.co.za/products/Nike-Air-More-Money-Olive",
                2399,
                "pics/product/large/aj2998-200-side.jpg",
                "id");


            ShelflifeScraper scraper = new ShelflifeScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
    }
    
}
