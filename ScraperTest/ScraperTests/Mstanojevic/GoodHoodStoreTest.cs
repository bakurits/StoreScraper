using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.GoodHoodStore;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class GoodHoodStoreTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            GoodHoodStoreScrapper scraper = new GoodHoodStoreScrapper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike";

            
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new GoodHoodStoreScrapper(), "Zoom Fly BeTrue - White/Black-Palest Purple",
                "https://goodhoodstore.com/store/nike-zoom-fly-betrue-white-black-palest-purple-38067",
                420,
                "https://assets.cdn.goodhoodstore.com/products/38544/medium/VANS_GOODHOOD_AW17_147-2.jpg",
                "id");


            GoodHoodStoreScrapper scraper = new GoodHoodStoreScrapper();

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
