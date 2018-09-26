using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.GiorgiChkhikvadze;

namespace ScraperTest.ScraperTests.GiorgiChkhikvadze
{
    [TestClass]
    public class OffWhiteTest
    {
        public OffWhiteScraper Scraper = new OffWhiteScraper();

        [TestMethod]
        public void TestFind()
        {
            var searchSettings = Helper.SearchSettings;
            Scraper.FindItems(out var listOfProducts, searchSettings, CancellationToken.None);
            Helper.PrintFindItemsResults(listOfProducts);
        }


        [TestMethod]
        public void TestGetDetails()
        {

            var details = new OffWhiteScraper().GetProductDetails("https://www.off---white.com/en/GE/men/products/omel003e180260109901#image-0", CancellationToken.None);
            Console.WriteLine(details);
        }

        [TestMethod]
        public void TestGetDetails2()
        {
            var scraper = new OffWhiteScraper {Active = true};
            var details = scraper.GetProductDetails("https://www.off---white.com/en/GE/men/products/omel003e180260109901#image-0", CancellationToken.None);

            Console.WriteLine(string.Join(" ", details.SizesList));
        }
    }
}
