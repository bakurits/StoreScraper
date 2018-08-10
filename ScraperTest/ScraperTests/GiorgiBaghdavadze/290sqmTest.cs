using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.GiorgiBaghdavadze._290sqm;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.GiorgiBaghdavadze
{
    [TestClass]
    public class _290sqmTest
    {
        private IstSqm scraper = new IstSqm();
        public IstSqm Scraper { get => scraper; set => scraper = value; }

        [TestMethod]
        public void TestFind()
        {

        }

        [TestMethod]
        public void TestGetProductDetails()
        {
            var product = new Product()
            {
                Url = "https://ist.290sqm.com/index.php?route=product/product&product_id=12445&search=t-shirt",
                ScrapedBy = Scraper
            };

            var details = product.GetDetails(CancellationToken.None);

            foreach (var size in details.SizesList)
            {
                Console.WriteLine(size);
            }

        }
    }
}
