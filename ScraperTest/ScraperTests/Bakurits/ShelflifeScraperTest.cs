﻿using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Shelflife;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass()]
    public class ShelflifeScraperTest
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
            Helper.PrintFindItemsResults(lst);
            
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

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            
            Helper.PrintGetDetailsResult(details.SizesList);
            
        }
        
        
        [TestMethod]
        public void GetNewArrivalsPage()
        {
            ShelflifeScraper scraper = new ShelflifeScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}