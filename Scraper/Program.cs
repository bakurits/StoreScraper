using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using StoreScraper.Bots.Footaction;
using StoreScraper.Bots.OffWhite;
using StoreScraper.Browser;
using StoreScraper.Controls;

namespace StoreScraper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppSettings.Init();
            if (!Directory.Exists(AppSettings.DataDir)) Directory.CreateDirectory(AppSettings.DataDir);
            AppSettings.Default = AppSettings.Load();


            LoadingForm loadingForm = new LoadingForm();
            loadingForm.Show();
         
            CookieCollector.Default = new CookieCollector();
            
            AppSettings.Default.AvaibleBots = new List<ScraperBase>
            {
                new OffWhiteBot(),
                new FootactionScrapper(),
            };

            

            loadingForm.Close();
            Application.Run(new MainForm());
        }


        static void OnProcessExit(object sender, EventArgs e)
        {
            CookieCollector.Default.Dispose();
        }
    }
}
