using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Dtlr;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{

    [TestClass]
    public class DtlrTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            DtlrScrapper scraper = new DtlrScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new DtlrScrapper(), "Unknown",
                "https://www.dtlr.com/nike-air-huarache-run-ultra-gs-847569-600.html",
                0,
                "",
                "id");


            DtlrScrapper scraper = new DtlrScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
