using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.GiorgiBaghdavadze.Adidas;
using StoreScraper.Bots.Html.GiorgiBaghdavadze.Nordstrom;
using StoreScraper.Models;
namespace ScraperTest.ScraperTests.GiorgiBaghdavadze
{
    [TestClass]

    public class AdidasSraperTest
    {

        public AdidasSraper Scraper { get; set; } = new AdidasSraper();

        [TestMethod]
        public void TestGetProductDetails()
        {
            var product = new Product()
            {
                Url = "https://www.adidas.com/us/pod-s3.1-shoes/B37458.html",
                ScrapedBy = Scraper
            };

            var details = product.GetDetails(CancellationToken.None);

            foreach (var size in details.SizesList)
            {
                Console.WriteLine(size.Key);

            }

        }

    }
}
