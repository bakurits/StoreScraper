using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Nighshop;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class NighShopTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            NighshopScraper scraper = new NighshopScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }
        [TestMethod()]
        public void GetProductDetailsTest()
        {
            NighshopScraper scraper = new NighshopScraper();
            Product curProduct = new Product(scraper,
                "Nike Air Jordan Wmns 1 Retro",
                "https://www.nighshop.com/adidas-by-raf-simons-replicant-ozweego-belgium-schwarz-rot-gelb-f34234",
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
            NighshopScraper scraper = new NighshopScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
