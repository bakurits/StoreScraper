using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Solebox;
using StoreScraper.Models;
namespace ScraperTest.Tests
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
