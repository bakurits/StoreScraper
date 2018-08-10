using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Sneakers76;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{

    [TestClass]
    public class Sneakers76Test
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            Sneakers76Scrapper scraper = new Sneakers76Scrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new Sneakers76Scrapper(), "Unknown",
                "https://www.sneakers76.com/en/nike/2993-atmos-x-nike-air-max-95-we-love-nike-aq0925-002.html?search_query=nike&results=336",
                0,
                "",
                "id");


            Sneakers76Scrapper scraper = new Sneakers76Scrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
