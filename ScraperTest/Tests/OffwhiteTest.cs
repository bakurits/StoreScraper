using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.OffWhite;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class OffWhiteTest
    {
        public OffWhiteScraper Scraper = new OffWhiteScraper();

        [TestMethod]
        public void TestFind()
        {
            var searchSettings = Helper.SearchSettings;
            Scraper.FindItems(out var listOfProducts, searchSettings, CancellationToken.None);
            Helper.PrintTestReuslts(listOfProducts);
        }


        [TestMethod]
        public void TestGetDetails()
        {
            var testProduct = new Product(){Id = "/en/GE/men/products/omca072e188200111001.json" };
            var details = new OffWhiteScraper().GetProductDetails(testProduct,CancellationToken.None);

            Console.WriteLine(string.Join(" ",details.SizesList));
        }

        [TestMethod]
        public void TestGetDetails2()
        {
            var testProduct = new Product() { Id = "/en/GE/men/products/omca072e188200111001.json" };
            var scraper = new OffWhiteScraper {Active = true};
            var details = scraper.GetProductDetails(testProduct, CancellationToken.None);

            Console.WriteLine(string.Join(" ", details.SizesList));
        }
    }
}
