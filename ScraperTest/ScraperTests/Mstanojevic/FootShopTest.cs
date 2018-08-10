﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.FootShop;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class FootShopTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            FootShopScrapper scraper = new FootShopScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "vault";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new FootShopScrapper(), "Unknown",
                "https://www.footshop.eu/en/mens-shoes/31146-converse-one-star-ox-apple-green-sharp-green.html",
                420,
                "https://1256852360.rsc.cdn77.org/en/170459/converse-one-star-ox-apple-green-sharp-green.jpg",
                "id");


            FootShopScrapper scraper = new FootShopScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}