using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Snkrs;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class SnkrsTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            SnkrsScrapper scraper = new SnkrsScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new SnkrsScrapper(), "Unknown",
                "http://www.snkrs.com/en/nike/m2k-tekno-blackoff-white-8222.html",
                420,
                "http://media.snkrs.com/43202-thickbox/m2k-tekno-blackoff-white.jpg",
                "id");


            SnkrsScrapper scraper = new SnkrsScrapper();

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
