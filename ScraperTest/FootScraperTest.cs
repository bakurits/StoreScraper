using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
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

        [AssemblyInitialize]
        public static void InitSettings(TestContext context)
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            settings = new SearchSettingsBase()
            {
                KeyWords = "blue shirt"
            };
        }
    }
}