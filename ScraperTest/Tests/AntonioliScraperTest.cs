using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Antonioli;
using StoreScraper.Bots.Shelflife;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class AntonioliScraperTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            AntonioliScraper scraper = new AntonioliScraper();
            AntonioliSearchSettingsBase settings = new AntonioliSearchSettingsBase()
            {
                KeyWords = "watch",
                Gender = AntonioliSearchSettingsBase.GenderEnum.Men
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new AntonioliScraper(), "INCOTEX Slim - Fit Pleated Brushed Stretch - Cotton Trousers",
                "https://www.shelflife.co.za/products/Nike-Air-More-Money-Olive",
                2399,
                "pics/product/large/aj2998-200-side.jpg",
                "id");


            AntonioliScraper scraper = new AntonioliScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
    }
}
