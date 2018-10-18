using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Urbanjunglestore;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class Urbanjunglestoretest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            UrbanjunglestoreScraper scraper = new UrbanjunglestoreScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }
        [TestMethod()]
        public void GetProductDetailsTest()
        {
            UrbanjunglestoreScraper scraper = new UrbanjunglestoreScraper();
            Product curProduct = new Product(scraper, 
                "Nike Air Jordan Wmns 1 Retro",
                "https://www.urbanjunglestore.com/it/nike-wmns-air-jordan-1-retro-low-ns-ah7232-623.html",
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
            UrbanjunglestoreScraper scraper = new UrbanjunglestoreScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }

        // Nike Air Jordan Wmns 1 Retro
        // "https://www.urbanjunglestore.com/it/nike-wmns-air-jordan-1-retro-low-ns-ah7232-623.html"
        // new Product(this, name, url, price, imageUrl, url, "EUR")
    }
    
    

   
}
