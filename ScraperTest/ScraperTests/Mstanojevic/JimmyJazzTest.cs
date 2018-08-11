using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Mstanojevic.JimmyJazz;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class JimmyJazz
    {
        [TestMethod]
        public void FindItemsTest()
        {
            JimmyJazzScraper scraper = new JimmyJazzScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "puma"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new JimmyJazzScraper(), "AIR MAX 95 OG",
                "http://www.jimmyjazz.com/mens/footwear/nike-air-max-95-og/AT2865-100?color=White",
                420,
                "https://44a54cd7e43cae68d339-79fdfac25b5b7a089d2cf87c8db56622.ssl.cf2.rackcdn.com/AH8396-102/AH8396-102_white_1000_1.jpg",
                "id");


            JimmyJazzScraper scraper = new JimmyJazzScraper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Debug.WriteLine(details.Name);
            Debug.WriteLine(details.Price);
            Debug.WriteLine(details.Currency);
            Debug.WriteLine(details.ImageUrl);
            Debug.WriteLine(details.StoreName);
            Debug.WriteLine(details.Url);

            Helpers.Helper.PrintGetDetailsResult(details.SizesList);

        }

    }
}
