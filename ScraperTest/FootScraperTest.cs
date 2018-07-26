using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            FootStoreScraper.ChampsSportsScraper scraper = new FootStoreScraper.ChampsSportsScraper();
            scraper.FindItems(out  var lst, settings, CancellationToken.None, new Logger());
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void FootLocker()
        {
            FootStoreScraper.FootLockerScraper scraper = new FootStoreScraper.FootLockerScraper();
            scraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
            Helper.PrintTestReuslts(lst);
        }

        [TestMethod]
        public void EastBay()
        {
            FootStoreScraper.EastBayScraper footStoreScraper = new FootStoreScraper.EastBayScraper();
            footStoreScraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
            Helper.PrintTestReuslts(lst);
        }

        [AssemblyInitialize]
        public static void InitSettings(TestContext context)
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            settings = new SearchSettingsBase()
            {
                KeyWords = "white"
            };
        }
    }
}