using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Scrapers.Footaction;
using StoreScraper.Models;

namespace ScraperTest
{
    [TestClass]
    public class FootActionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
           var searchSettingsBase  = new SearchSettingsBase()
            {
                MaxPrice = 0,
                MinPrice = 0,
                KeyWords = "white t-shirt",
                NegKeyWrods = "",
            };
            new FootactionScrapper().FindItems(out var listOfProducts, searchSettingsBase,CancellationToken.None,new Logger());
        }


        [TestInitialize]
        public  void Init()
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();
        }
    }
}
