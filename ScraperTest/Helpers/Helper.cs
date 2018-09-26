using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FootLocker_FootAction;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Http;
using StoreScraper.Models;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace ScraperTest.Helpers
{
    [TestClass]
    public class Helper
    {
       public static readonly SearchSettingsBase SearchSettings = new SearchSettingsBase
        {
            KeyWords = "blue t-shirt",
            NegKeyWords = "Woman",
            MinPrice = 200,
            MaxPrice = 1000,
        };

        public static readonly FootApiSearchSettings FootApiSearchSettingsSearchSettings = new FootApiSearchSettings
        {
            Gender = FootApiSearchSettings.GenderEnum.Any,
            KeyWords = "blue t-shirt",
            NegKeyWords = "Woman",
            MinPrice = 200,
            MaxPrice = 1000,
        };
        
        public static void PrintFindItemsResults<T>(List<T> list)
        {
            Console.WriteLine(string.Join("\n", list));
        }

        public static void PrintGetDetailsResult(List<StringPair> sizes)
        {
            Console.WriteLine(string.Join("\n", sizes.Select(size => $"{size.Key}[{size.Value}]")));
        }

        [AssemblyInitialize]
        public static void InitSettings(TestContext context)
        {
            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            Logger.Instance.OnLogged += (message, color) => Console.WriteLine(message);
            CookieCollector.Default = new CookieCollector();
        }
    }
}
