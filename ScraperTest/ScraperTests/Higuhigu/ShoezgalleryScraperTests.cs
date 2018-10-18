using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Shoezgallery;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class ShoezgalleryScraperTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            ShoezgalleryScraper scraper = new ShoezgalleryScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            ShoezgalleryScraper scraper = new ShoezgalleryScraper();
            Product curProduct = new Product(scraper,
                "Nike Air Jordan Wmns 1 Retro",
                "https://www.shoezgallery.com/en/p9727-plato-sneaker-satellite-no-name-plato-sneaker-bandana-black?search_query=sneaker&results=13",
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
            ShoezgalleryScraper scraper = new ShoezgalleryScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

    }

    //https://www.shoezgallery.com/en/p9727-plato-sneaker-satellite-no-name-plato-sneaker-bandana-black?search_query=sneaker&results=13
}
