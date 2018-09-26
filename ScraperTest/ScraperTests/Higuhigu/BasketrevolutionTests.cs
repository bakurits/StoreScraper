using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Html.Higuhigu.Basketrevolution;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class BasketrevolutionTests
    {
        [TestMethod]
        public void FindItemsTest()
        {
            BasketrevolutionScraper scraper = new BasketrevolutionScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            BasketrevolutionScraper scraper = new BasketrevolutionScraper();
            Product curProduct = new Product(scraper,
                "Wut",
                "https://www.basketrevolution.es/jordan-super-fly-2017-black-infrared/",
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
    }
}
