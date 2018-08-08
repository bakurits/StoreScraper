using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.Einhalb;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class EinhalbScraperTest
    {
        [TestMethod]
        public void FindItemsTest()
        {
            EinhalbScraper scraper = new EinhalbScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "jordan"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }
    }
}
