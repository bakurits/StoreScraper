using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Baitme;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class BaitmeScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            BaitmeScraper scraper = new BaitmeScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jacket"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
        [TestMethod]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new BaitmeScraper(), "Unknown",
                "https://www.baitme.com/vans-men-authentic-studded-stars-red-blue-vavn018bh0f-35",
                85.00,
                "https://d3hed5rtv63hp1.cloudfront.net/products/280972/large/JC8230W115-BLACK-6014.jpg?1519741368",
                "id");


            BaitmeScraper scraper = new BaitmeScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.ImageUrl);

        }
        
        [TestMethod]
        public void GetNewArrivalsPage()
        {
            BaitmeScraper scraper = new BaitmeScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
