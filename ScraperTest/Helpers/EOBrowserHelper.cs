using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.Core;
using EO.WebBrowser;

namespace ScraperTest.Helpers
{
    // ReSharper disable once InconsistentNaming
    public static class EOBrowserHelper
    {   


        public static TResult BotTester<TResult, T>(T bot, Func<T, TResult> action, string proxyAddr = null) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TResult flag = default(TResult);
            Task.Delay(5000).ContinueWith(delay =>
            {
                flag = action(bot);
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            });

            bot.Browser =  new EOBrowserDriver();
            WebView.ShowDebugUI();
            bot.Browser.ShowDialog();
            return flag;
        }
        
        
        public static void BotTester<T>(T bot, System.Action<T> action) where T : FootSitesBotBase
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Task.Delay(10000).ContinueWith(delay =>
            {  
                action(bot);
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            });
            
            bot.Browser =  new EOBrowserDriver();
            WebView.ShowDebugUI();
            bot.Browser.ShowDialog();
        }



    }
}
