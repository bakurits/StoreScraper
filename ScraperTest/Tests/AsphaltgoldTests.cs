﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Asphaltgold;
using StoreScraper.Models;
namespace ScraperTest.Tests
{
    [TestClass]
    public class AsphaltgoldTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            AsphaltgoldScraper scraper = new AsphaltgoldScraper();
            AsphaltgoldSearchSettings settings = new AsphaltgoldSearchSettings()
            {
                KeyWords = "jordan",
                ItemType = 0
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }


        [TestMethod()]
        public void GetProductDetailsTest()
        {
            AsphaltgoldScraper scraper = new AsphaltgoldScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://asphaltgold.de/en/nike-s-s-top-taped-poly-sail-black.html",
                1,
                "whatever",
                "whatever",
                "EUR");

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }

    }


}