using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Sneakersnstuff;
using StoreScraper.Models;
namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class SneakersnstuffTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            SneakersnstuffScrapper scraper = new SneakersnstuffScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            //settings.KeyWords = "nike";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void NewArrivalTest()
        {
            SneakersnstuffScrapper scraper = new SneakersnstuffScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";


            scraper.ScrapeNewArrivalsPage(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new SneakersnstuffScrapper(), "Unknown",
                "https://www.sneakersnstuff.com/en/product/30856/nike-air-safari-se",
                420,
                "https://www.sneakersnstuff.com/images/205337/large.jpg",
                "id");


            SneakersnstuffScrapper scraper = new SneakersnstuffScrapper();

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
