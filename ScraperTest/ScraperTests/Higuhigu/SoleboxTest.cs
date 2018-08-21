using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Higuhigu.Solebox;
using StoreScraper.Models;

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
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

        [TestMethod()]
        public void GetProductDetailsTest()
        {
            SoleboxScraper scraper = new SoleboxScraper();
            ProductDetails details = scraper.GetProductDetails("https://www.solebox.com/en/Footwear/Basketball/Air-Jordan-1Retro-Hi-Premium.html", CancellationToken.None);
            foreach (var sz in details.SizesList)
            {
                Debug.WriteLine(sz);
            }
        }
    }
}
