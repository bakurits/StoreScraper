using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Mstanojevic.UebervartShop;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class UebervartShopTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            UebervartShopScrapper scraper = new UebervartShopScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void NewArrivalTest()
        {
            UebervartShopScrapper scraper = new UebervartShopScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";


            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new UebervartShopScrapper(), "Unknown",
                "https://www.uebervart-shop.de/nike-air-revarderchi-red-brown/",
                420,
                "",
                "id");


            UebervartShopScrapper scraper = new UebervartShopScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct.Url, CancellationToken.None);

            Debug.WriteLine(details.Name);
            Debug.WriteLine(details.Price);
            Debug.WriteLine(details.Currency);
            Debug.WriteLine(details.ImageUrl);
            Debug.WriteLine(details.StoreName);
            Debug.WriteLine(details.Url);
            Debug.WriteLine(details.BrandName);

            Helper.PrintGetDetailsResult(details.SizesList);

        }
    }
}
