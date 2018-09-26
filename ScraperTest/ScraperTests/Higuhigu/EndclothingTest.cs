using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Html.Higuhigu.Endclothing;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class EndclothingTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            EndclothingScraper scraper = new EndclothingScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);
        }


        [TestMethod()]
        public void GetProductDetailsTest()
        {
            EndclothingScraper scraper = new EndclothingScraper();
            Product curProduct = new Product(scraper,
                "Whatever",
                "https://www.endclothing.com/us/air-jordan-12-retro-gg-510815-100.html",
                1,
                "whatever",
                "whatever",
                "EUR");

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Console.WriteLine(sz);
            }
        }
        
    }
}
