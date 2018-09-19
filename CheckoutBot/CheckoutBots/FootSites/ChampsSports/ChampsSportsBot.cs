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
            webView.EvalScript($"{getElementById("login_email")}.value='{username}'");
            webView.EvalScript($"{getElementById("login_password")}.value='{password}'");
            Task.Delay(10 * 1000).Wait(token);
            webView.EvalScript($"var a = document.getElementById('loginIFrame').contentDocument.getElementsByClassName('button'); a[0].click()");
            Debug.WriteLine(webView.LastJSException.ToString());
            Task.Delay(10 * 1000, token).Wait(token);
            return false;
        }

        private string getElementById(string id)
        {
            return
                $"document.getElementById('loginIFrame').contentDocument.getElementById('{id}')";
        }
    }
}
