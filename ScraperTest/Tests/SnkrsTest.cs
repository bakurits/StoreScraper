using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Snkrs;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class SnkrsTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            SnkrsScrapper scraper = new SnkrsScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new SnkrsScrapper(), "Unknown",
                "https://www.snkrs.com/en/nike/w-air-max-1-100-white-7953.html?search_query=nike&results=89",
                420,
                "https://media2.snkrs.com/42366-thickbox/w-air-max-1-100-white.jpg",
                "id");


            SnkrsScrapper scraper = new SnkrsScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
