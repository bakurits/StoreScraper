using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Html.Mstanojevic.JimmyJazz;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

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
                KeyWords = "nike"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod]
        public void NewArrivalTest()
        {
            JimmyJazzScraper scraper = new JimmyJazzScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "nike"
            };

            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new JimmyJazzScraper(), "AIR MAX 95 OG",
                "https://www.jimmyjazz.com/mens/footwear/nike-air-max-95-og/AT2865-100?color=White",
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
