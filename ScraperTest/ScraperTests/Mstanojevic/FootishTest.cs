﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mstanojevic.Footish;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class FootishTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            FootishScrapper scraper = new FootishScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new FootishScrapper(), "Unknown",
                "https://www.footish.se/en/sneakers/nike-wmns-air-max-90-essential-616730-111",
                420,
                "https://www.footish.se/pub_images/large/nike-air-max-2017-at0044-001-p21554.jpg",
                "id");


            FootishScrapper scraper = new FootishScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);
            Helpers.Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);
            
        }
    }
}