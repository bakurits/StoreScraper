using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.JimmyJazz;
using StoreScraper.Models;

namespace ScraperTest.Tests
{
    [TestClass]
    public class JimmyJazz
    {
        [TestMethod]
        public void FindItemsTest()
        {
            JimmyJazzScraper scraper = new JimmyJazzScraper();
            SearchSettingsBase settings = new SearchSettingsBase()
            {
                KeyWords = "puma"
            };

            scraper.FindItems(out var lst, settings, CancellationToken.None);
        }

    }
}
