﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Cruvoir;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class CruvoirTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            CruvoirScrapper scraper = new CruvoirScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "vault";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new CruvoirScrapper(), "Unknown",
                "https://cruvoir.com/collections/sneakers/products/black-pod-s3-1-sneaker",
                420,
                "https://cdn.shopify.com/s/files/1/2666/2006/products/13029260_13855022_1000_1400x.jpg?v=1529190499",
                "id");


            CruvoirScrapper scraper = new CruvoirScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
