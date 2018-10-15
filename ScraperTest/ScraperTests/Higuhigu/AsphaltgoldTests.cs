using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Asphaltgold;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
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
                ItemType = AsphaltgoldSearchSettings.ItemTypeEnum.Sneakers
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

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
        
        [TestMethod]
        public void ScrapeAllProductsTest()
        {
            AsphaltgoldScraper scraper = new AsphaltgoldScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

    }


}
