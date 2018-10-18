using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScraperTest.Helpers;
using StoreScraper.Bots.Html.Higuhigu.Solebox;
using StoreScraper.Models;
using StoreScraper.Models.Enums;

namespace ScraperTest.ScraperTests.Higuhigu
{
    [TestClass]
    public class SoleboxTest
    {
        [TestMethod()]
        public void FindItemsTest()
        {
            SoleboxScraper scraper = new SoleboxScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "Air+Presto"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            SoleboxScraper scraper = new SoleboxScraper();
            ProductDetails details = scraper.GetProductDetails("https://www.solebox.com/en/Footwear/Basketball/Air-Jordan-1-Retro-High-OG-variant-14.html", CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
        
        [TestMethod]
        public void ScrapeAllProductsTest()
        {
            SoleboxScraper scraper = new SoleboxScraper();
            
            scraper.ScrapeAllProducts(out var lst, ScrappingLevel.PrimaryFields, CancellationToken.None);
            Helper.PrintFindItemsResults(lst);
        }
    }
}
