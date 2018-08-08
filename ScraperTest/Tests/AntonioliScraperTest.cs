using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Antonioli;
using StoreScraper.Bots.Shelflife;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class AntonioliScraperTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            AntonioliScraper scraper = new AntonioliScraper();
            AntonioliSearchSettings settings = new AntonioliSearchSettings()
            {
                KeyWords = "Boots",
                Gender = AntonioliSearchSettings.GenderEnum.Woman
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new AntonioliScraper(), "Unknown",
                "https://www.antonioli.eu/en/GE/women/products/jc8230w115-black",
                420,
                "https://d3hed5rtv63hp1.cloudfront.net/products/280972/large/JC8230W115-BLACK-6014.jpg?1519741368",
                "id");


            AntonioliScraper scraper = new AntonioliScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
