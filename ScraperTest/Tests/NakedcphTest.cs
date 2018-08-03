using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Nakedcph;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class NakedcphTest
    {
        public NakedcphScrapper scrapper = new NakedcphScrapper();

        [TestMethod]
        public void TestFind()
        {
            var searchSettingsBase = new SearchSettingsBase()
            {
                MaxPrice = 0,
                MinPrice = 0,
                KeyWords = "white t-shirt",
                NegKeyWrods = "",
            };
            scrapper.FindItems(out var listOfProducts, searchSettingsBase, CancellationToken.None);
        }

        [TestMethod]
        public void TestGetProductDetails()
        {
            var product = new Product()
            {
                Url = "https://www.nakedcph.com/adidas-originals-t-shirt-ce1666/p/6724",
                ScrapedBy = scrapper
            };

            var details = product.GetDetails(CancellationToken.None);

            Console.WriteLine(string.Join(", ", details.SizesList));
        }
    }
}