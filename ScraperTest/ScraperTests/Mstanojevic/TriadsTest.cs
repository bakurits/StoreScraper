﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Triads;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class TriadsTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            TriadsScrapper scraper = new TriadsScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "vault";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new TriadsScrapper(), "Unknown",
                "https://www.triads.co.uk/triads-mens-c1/footwear-c24/trainers-c211/thunder-desert-bright-white-p85358",
                420,
                "https://www.triads.co.uk/images/products/medium/1533740596-00841700.jpg",
                "id");


            TriadsScrapper scraper = new TriadsScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}