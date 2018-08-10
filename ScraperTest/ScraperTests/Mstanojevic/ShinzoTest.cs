﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Shinzo;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class ShinzoTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            ShinzoScrapper scraper = new ShinzoScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new ShinzoScrapper(), "Unknown",
                "https://www.shinzo.paris/en/running-shoes/111-nike-air-zoom-vomero-12-863762-001.html?search_query=nike&results=906",
                420,
                "",
                "id");


            ShinzoScrapper scraper = new ShinzoScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}