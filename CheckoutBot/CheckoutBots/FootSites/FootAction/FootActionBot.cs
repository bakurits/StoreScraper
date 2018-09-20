using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models.Checkout;
using CheckoutBot.Models.Shipping;

namespace CheckoutBot.CheckoutBots.FootSites.FootAction
{
    public class FootActionBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/34";
        public int DelayInSecond { private get; set; } = 2;

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
          
        }

        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrlAndWait(WebsiteBaseUrl);
            webView.EvalScript($"{GetScriptByXpath("//div[@class='c-header-ribbon__user']/button")}.click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            webView.EvalScript($"{GetScriptByXpath("//input[@name='email.email']")}.value=\"{username}\"");
            webView.EvalScript($"{GetScriptByXpath("//input[@name='password.password']")}.value=\"{password}\"");
            webView.EvalScript($"{GetScriptByXpath("//div[contains(@class,'c-sign-in-form__actions')]//button[contains(@class,'c-btn--primary')]")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);

            string isWrong = (string)webView.EvalScript(GetScriptByXpath("//p[@class='c-alert error']"));
            return isWrong.Length == 0;
        }

        public FootActionBot() : base("FootAction", "https://www.footaction.com/", ApiUrl)
        {
        }
    }
}
