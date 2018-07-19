using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckOutBot.Bots;
using CheckOutBot.Browser;
using CheckOutBot.Controls;
using CheckOutBot.Core;
using CheckOutBot.Helpers;
using CheckOutBot.Interfaces;
using CheckOutBot.Properties;
using OpenQA.Selenium;

namespace CheckOutBot
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
                new OffWhiteBot()
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
