using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Http;
using StoreScraper.Models;

namespace ScraperTest
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
            Console.WriteLine(string.Join("\n", list));
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
