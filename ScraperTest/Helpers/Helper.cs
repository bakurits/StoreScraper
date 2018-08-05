using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Http;
using StoreScraper.Models;

namespace ScraperTest.Helpers
{
    [TestClass]
    public class Helper
    {
       public static SearchSettingsBase SearchSettings = new SearchSettingsBase
        {
            KeyWords = "blue t-shirt",
            NegKeyWrods = "Woman",
            MinPrice = 200,
            MaxPrice = 1000,
        };

        public static void PrintTestReuslts(List<Product> list)
        {
            Debug.WriteLine(string.Join("\n", list));
        }

        public static void PrintGetDetailsResult(List<String> sizes)
        {
            Debug.WriteLine(string.Join("\n", sizes));
        }

        [AssemblyInitialize]
        public static void InitSettings(TestContext context)
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            Logger.Instance.OnLogged += (message, color) => Debug.WriteLine(message);
            CookieCollector.Default = new CookieCollector();
        }
    }
}
