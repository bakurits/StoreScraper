using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Bakurits.Baitme;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class BaitmeScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            BaitmeScraper scraper = new BaitmeScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jacket"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new BaitmeScraper(), "Unknown",
                "http://www.baitme.com/bait-coach-jacket-green-babt140205-005grn",
                85.00,
                "https://d3hed5rtv63hp1.cloudfront.net/products/280972/large/JC8230W115-BLACK-6014.jpg?1519741368",
                "id");


            BaitmeScraper scraper = new BaitmeScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.ImageUrl);

        }
    }
}
