using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Mstanojevic.Ntrstore;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Mstanojevic
{
    [TestClass]
    public class NtrstoreTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            NtrstoreScraper scraper = new NtrstoreScraper();
            SearchSettingsBase settings = new SearchSettingsBase();
            settings.KeyWords = "nike air";


            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helpers.Helper.PrintFindItemsResults(lst);

        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
        /*Product curProduct = new Product(new NtrstoreScraper(), "Unknown",
            "https://www.ntrstore.com/nike-flyknit-lunar-3",
            420,
            "",
            "id");


        NtrstoreScraper scraper = new NtrstoreScraper();

        ProductDetails details = scraper.GetProductDetails(curProduct, CancellationToken.None);

        Helper.PrintGetDetailsResult(details.SizesList);
        Debug.WriteLine(curProduct.Name);

        */


        

            Product curProduct = new Product(new NtrstoreScraper(), "Unknown",
            "https://www.ntrstore.com/air-180-og-white-ultramarine",
            420,
            "",
            "id");


            NtrstoreScraper scraper = new NtrstoreScraper();

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
