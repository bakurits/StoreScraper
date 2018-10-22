using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scraper.Controls;
using StoreScraper;
using StoreScraper.Core;
using StoreScraper.Data;
using StoreScraper.Helpers;
using StoreScraper.Http.CookieCollecting;

namespace Scraper
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

            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();

            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            Session.Current.AvailableScrapers = Utils.GetAllSubClassInstances<ScraperBase>().ToList();
            Session.Current.AvailableScrapers.Sort((s1, s2) => string.CompareOrdinal(s1.WebsiteName, s2.WebsiteName));

            Application.Run(new MainForm());
        }


        private static bool EnsureInternetConnection()
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
