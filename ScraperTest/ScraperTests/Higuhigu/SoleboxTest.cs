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
    }
}
