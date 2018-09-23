﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;
using StoreScraper.Attributes;
using StoreScraper.Controls;
using StoreScraper.Helpers;
using StoreScraper.Http;

namespace StoreScraper
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();

            Application.SetCompatibleTextRenderingDefault(false);

            EnsureInternetConnection();

            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            CookieCollector.Default = new CookieCollector();
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            AppSettings.Default.AvailableScrapers = Utils.GetAllSubClassInstances<ScraperBase>().ToList();
            AppSettings.Default.AvailableScrapers.Sort((s1, s2) => string.CompareOrdinal(s1.WebsiteName, s2.WebsiteName));

            Application.Run(new MainForm());
        }


        public static bool EnsureInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("https://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                throw new WebException("Internet Not Available");
            }
        }
    }
}
