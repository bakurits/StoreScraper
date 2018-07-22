using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Bots.Mrporter;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class Mrporter
    {
        [TestMethod]
        public void TestMethod1()
        {
            MrporterScraper scraper = new MrporterScraper();
            MrporterSearchSettings settings = new MrporterSearchSettings()
            {
                KeyWords = "sneakers"
            };
           
            scraper.FindItems(out var lst, settings, CancellationToken.None, new Logger());
        }


        [TestInitialize]
        public void Init()
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();
        }
    }
}
