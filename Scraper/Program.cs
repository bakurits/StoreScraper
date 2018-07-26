using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using StoreScraper.Attributes;
using StoreScraper.Scrapers.Mrporter;
using StoreScraper.Scrapers.OffWhite;
using StoreScraper.Browser;
using StoreScraper.Controls;

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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            LoadingForm form = new LoadingForm();
            form.Show();

            AppSettings.Default.AvaibleBots = GetScrapers().ToList();

            form.Close();

            Application.Run(new MainForm());
        }


        static void OnProcessExit(object sender, EventArgs e)
        {
            CookieCollector.Default.Dispose();
        }

        public static IEnumerable<ScraperBase> GetScrapers()
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(ScraperBase)))
                {
                    bool disabled =
                        type.CustomAttributes.Any(attr => attr.AttributeType == typeof(DisabledScraperAttribute));
                    if (!disabled)
                    {
                        yield return (ScraperBase) Activator.CreateInstance(type);
                    }
                }
            }
        }


        public static bool EnsureInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
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
