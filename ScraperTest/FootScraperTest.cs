using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper.Bots.ChampsSports_FootLocker_EastBay;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class FootScraperTest
    {
        public static SearchSettingsBase settings;

        [TestMethod]
        public void ChampsSportsScraper()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("ChampsSports", "https://www.champssports.com");
            footStoreScraper.FindItems(out  var lst, settings, CancellationToken.None, new Logger());
        }

        [TestMethod]
        public void FootLocker()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("FootLocker", "https://www.footlocker.com");
            footStoreScraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
        }

        [TestMethod]
        public void EastBay()
        {
            FootStoreScraper footStoreScraper = new FootStoreScraper("EastBay", "https://www.eastbay.com");
            footStoreScraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
        }

        [ClassInitialize]
        public static void InitSettings(TestContext context)
        {
            settings = new SearchSettingsBase()
            {
                KeyWords = "blue shirt"
            };
        }
    }
}