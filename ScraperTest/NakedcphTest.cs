using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Models;
using StoreScraper.Bots.Nakedcph;
namespace ScraperTest
{
    [TestClass]
    public class NakedcphTest
    {
        public NakedcphScrapper scrapper = new NakedcphScrapper();

        [TestMethod]
        public void TestMethod1()
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
    }
}
