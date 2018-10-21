using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Antonioli;
using StoreScraper.Bots.Html.Bakurits.Kith;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class KithScrapperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            KithScrapper scraper = new KithScrapper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "Boots"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            foreach (var item in lst)
            {
                Debug.WriteLine(item.Id);   
            }
        }

        [TestMethod]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new KithScrapper(), "Unknown",
                "https://kith.com/collections/latest/products/nike-air-jordan-10-retro-racer-blue-team-orange-black",
                420,
                "https://d3hed5rtv63hp1.cloudfront.net/products/280972/large/JC8230W115-BLACK-6014.jpg?1519741368",
                "id");


            KithScrapper scraper = new KithScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);
        }

        [TestMethod]
        public void GetNewArrivalsPage()
        {
            KithScrapper scraper = new KithScrapper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}