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
            settings.NegKeyWrods = "blue";
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new ExcelsiormilanoScrapper(), "Unknown",
                "https://www.excelsiormilano.com/shoes/34287-red-canvas-sneakers.html?search_query=red+canvas&results=3",
                420,
                "https://www.excelsiormilano.com/96579-large_default/red-canvas-sneakers.jpg",
                "id");


            ExcelsiormilanoScrapper scraper = new ExcelsiormilanoScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
