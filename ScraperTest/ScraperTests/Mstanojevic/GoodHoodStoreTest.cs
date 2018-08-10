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
            settings.KeyWords = "vault";

            
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            Product curProduct = new Product(new GoodHoodStoreScrapper(), "Unknown",
                "https://goodhoodstore.com/store/vans-vault-og-authentic-lx-dress-blues-wrought-iron-38544",
                420,
                "https://assets.cdn.goodhoodstore.com/products/38544/medium/VANS_GOODHOOD_AW17_147-2.jpg",
                "id");


            GoodHoodStoreScrapper scraper = new GoodHoodStoreScrapper();

            ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

            Helper.PrintGetDetailsResult(details.SizesList);
            Debug.WriteLine(curProduct.Name);

        }
    }
}
