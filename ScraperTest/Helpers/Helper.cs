﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StoreScraper;
using StoreScraper.Bots.Html.Sticky_bit.FootLocker_FootAction;
using StoreScraper.Core;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Http.CookieCollecting;
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
            Debug.WriteLine(string.Join("\n", list));
        }

        public static void PrintGetDetailsResult(List<StringPair> sizes)
        {
            Debug.WriteLine(string.Join("\n", sizes.Select(size => $"{size.Key}[{size.Value}]")));
        }

        [AssemblyInitialize]
        public static void InitSettings(TestContext context)
        {
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();
            Session.Current.AvailableScrapers = Utils.GetAllSubClassInstances<ScraperBase>().ToList();
            Session.Current.AvailableScrapers.Sort((s1, s2) => string.CompareOrdinal(s1.WebsiteName, s2.WebsiteName));
            Logger.Instance.OnLogged += (message, color) => Console.WriteLine(message);
            CookieCollector.Default = new CookieCollector();
        }
    }
}
