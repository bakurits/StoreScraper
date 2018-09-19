using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Core;
using CheckoutBot.Models.Checkout;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.ChampsSports
{
    public class ChampsSportsBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/20";
        public int DelayInSecond { private get; set; } = 5;

        public ChampsSportsBot() : base("ChampsSports", "https://www.champssports.com/", ApiUrl)
        {
        }


        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Browser = new EOBrowserDriver();
            Browser.NewTab("MainTab");
            Login(settings.UserLogin, settings.UserPassword, token);
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            webView.EvalScript(GetScriptByXpath("//div[@id='header_login']") + ".click();");
            Task.Delay(10 * 1000, token).Wait(token);
            username = "ggg";
            webView.EvalScript($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            webView.EvalScript($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Debug.WriteLine(webView.LastJSException.ToString());
            //Driver.QueueScriptCall($"{GetScriptByXpath("//div[@id='header_login']/a//input[@id='login_submit']")}.click()");
            Task.Delay(100 * 1000, token).Wait(token);
            return false;
        }
    }
}
