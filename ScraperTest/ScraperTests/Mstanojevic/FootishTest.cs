using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mstanojevic.Footish;
using StoreScraper.Models;
using ScraperTest.Helpers;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class FootishTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            FootishScrapper scraper = new FootishScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new FootishScrapper(), "Unknown",
                "http://www.footish.se/en/sneakers/nike-air-footscape-nm-852629-001",
                420,
                "http://www.footish.se/pub_images/large/nike-air-max-2017-at0044-001-p21554.jpg",
                "30136");


            FootishScrapper scraper = new FootishScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);
            Debug.WriteLine(details.Name);
            Debug.WriteLine(details.Price);
            Debug.WriteLine(details.Currency);
            Debug.WriteLine(details.ImageUrl);
            Debug.WriteLine(details.StoreName);
            Debug.WriteLine(details.Url);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
    }
}
