using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Bakurits.Antonioli;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]     
    public class AntonioliScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            AntonioliScraper scraper = new AntonioliScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "Boots",
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
            Product curProduct = new Product(new AntonioliScraper(), "Unknown",
                "https://www.antonioli.eu/en/GE/women/products/jc8230w115-black",
                420,
                "https://d3hed5rtv63hp1.cloudfront.net/products/280972/large/JC8230W115-BLACK-6014.jpg?1519741368",
                "id");


            AntonioliScraper scraper = new AntonioliScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);
        }

        [TestMethod]
        public void GetNewArrivalsPage()
        {
            AntonioliScraper scraper = new AntonioliScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}