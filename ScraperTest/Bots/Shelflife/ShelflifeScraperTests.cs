using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Shelflife;
using StoreScraper.Models;

namespace ScraperTest.Bots.Shelflife
{
    [TestClass()]
    public class ShelflifeScraperTests
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            ShelflifeScraper scraper = new ShelflifeScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "watch"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            foreach (var item in lst)
            {
                Debug.WriteLine(item.Name);
                Debug.WriteLine(item.Url);
                Debug.WriteLine(item.ImageUrl);
                Debug.WriteLine(item.Price);
                Debug.WriteLine("");
                Debug.WriteLine("");
            }
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
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
    }
}