using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.GiorgiChkhikvadze.Nakedcph;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.GiorgiChkhikvadze
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
                NegKeyWords = "",
            };
            scrapper.FindItems(out var listOfProducts, searchSettingsBase, CancellationToken.None);
            Helper.PrintFindItemsResults(listOfProducts);
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

            Debug.WriteLine(details);
        }

        [TestMethod]
        public void TestScrapeNewArrivalsPage()
        {
            scrapper.ScrapeNewArrivalsPage(out var product, CancellationToken.None);
        }
    }
}