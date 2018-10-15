using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Chmielna;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class ChmielnaScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            ChmielnaScraper scraper = new ChmielnaScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            ChmielnaScraper scraper = new ChmielnaScraper();
            Product curProduct = new Product(scraper,
                "Nike Air Jordan Wmns 1 Retro",
                "https://chmielna20.pl/en/puma-x-fenty-chelsea-sneaker-boot-sterling-blue-36626601.html",
                1,
                "whatever",
                "whatever",
                "EUR");

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            Helper.PrintFindItemsResults(details.SizesList);
        }
        
        [TestMethod]
        public void ScrapeAllProductsTest()
        {
            ChmielnaScraper scraper = new ChmielnaScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
