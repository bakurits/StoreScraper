﻿using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Mstanojevic.Sneakers76;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Mstanojevic
{

    [TestClass]
    public class Sneakers76Test
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            Sneakers76Scrapper scraper = new Sneakers76Scrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void NewArrivalTest()
        {
            Sneakers76Scrapper scraper = new Sneakers76Scrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new Sneakers76Scrapper(), "Unknown",
                "https://www.sneakers76.com/en/nike/2993-atmos-x-nike-air-max-95-we-love-nike-aq0925-002.html?search_query=nike&results=336",
                0,
                "",
                "id");


            Sneakers76Scrapper scraper = new Sneakers76Scrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Debug.WriteLine(details.Name);
            Debug.WriteLine(details.Price);
            Debug.WriteLine(details.Currency);
            Debug.WriteLine(details.ImageUrl);
            Debug.WriteLine(details.StoreName);
            Debug.WriteLine(details.Url);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
    }
}
