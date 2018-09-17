using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Bakurits.Mrporter;
using StoreScraper.Models;

namespace ScraperTest.ScraperTests.Bakurits
{
    [TestClass]
    public class Mrporter
    {
        [TestMethod]
        public void TestMethod1()
        {
            MrporterScraper scraper = new MrporterScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "Yeezy"
            };
           
            scraper.FindItems(out var lst, settings, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
            
        }

        [TestMethod]
        public void TestMethod2()
        {
            Product curProduct = new Product(new MrporterScraper(), "JOHN ELLIOTT Camp-Collar Printed Tencel-Twill Shirt",
                "https://www.mrporter.com/mens/okeeffe/bristol-leather-trimmed-suede-derby-shoes/1026175",
                120.83,
                "https://cache.mrporter.com/images/products/1012326/1012326_mrp_in_l.jpg",
                "id");


            MrporterScraper scraper = new MrporterScraper();

            Helper.PrintGetDetailsResult(scraper.GetProductDetails(curProduct.Url, CancellationToken.None).SizesList);
        }

        [TestMethod]
        public void TestMethod3()
        {
            Product curProduct = new Product(new MrporterScraper(), "INCOTEX Slim - Fit Pleated Brushed Stretch - Cotton Trousers",
                "https://www.mrporter.com/en-ge/mens/incotex/slim-fit-pleated-brushed-stretch-cotton-trousers/1051390",
                120.83,
                "https://cache.mrporter.com/images/products/1012326/1012326_mrp_in_l.jpg",
                "id");


            MrporterScraper scraper = new MrporterScraper();

            Helper.PrintGetDetailsResult(scraper.GetProductDetails(curProduct.Url, CancellationToken.None).SizesList);
        }
        
        [TestMethod]
        public void GetNewArrivalsPage()
        {
            MrporterScraper scraper = new MrporterScraper();
            
            scraper.ScrapeNewArrivalsPage(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
