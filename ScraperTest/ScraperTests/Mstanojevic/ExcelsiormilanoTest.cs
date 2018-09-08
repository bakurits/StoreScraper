using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Excelsiormilano;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{

    [TestClass]
    public class ExcelsiormilanoTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            ExcelsiormilanoScrapper scraper = new ExcelsiormilanoScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "red canvas";
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void NewArrivalTest()
        {
            ExcelsiormilanoScrapper scraper = new ExcelsiormilanoScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "red canvas";
            scraper.ScrapeNewArrivalsPage(out var lst, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new ExcelsiormilanoScrapper(), "Unknown",
                "https://www.excelsiormilano.com/cat-url/36709-aztrek.html",
                420,
                "https://www.excelsiormilano.com/96579-large_default/red-canvas-sneakers.jpg",
                "id");


            ExcelsiormilanoScrapper scraper = new ExcelsiormilanoScrapper();

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
