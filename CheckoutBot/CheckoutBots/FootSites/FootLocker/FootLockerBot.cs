using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Models.Checkout;

namespace CheckoutBot.CheckoutBots.FootSites.FootLocker
{
    public class FootLockerBot : FootSitesBotBase
    {
        public override bool Login(string username, string password, CancellationToken token)
        {
            var webView = Browser.ActiveTab;

            webView.LoadUrlAndWait(WebsiteBaseUrl);
            webView.EvalScript(GetScriptByXpath("//button[text()='Sign In/VIP']") + ".click();");

            Task.Delay(5 * 1000, token).Wait(token);
            

            webView.EvalScript($"{GetScriptByXpath("//input[@type='EMAIL']")}.value=\"{username}\"");
            webView.EvalScript($"{GetScriptByXpath("//input[@type='PASSWORD']")}.value=\"{password}\"");
            Task.Delay(2 * 1000, token).Wait(token);
            webView.EvalScript($"{GetScriptByXpath("//button[text()='Sign In']")}.click()");
            Task.Delay(5 * 1000, token).Wait(token);
            
            string isWrong = (string) webView.EvalScript($"{GetScriptByXpath("//p[text()='Invalid email and/or password. Please try again.']")}");
            //Console.WriteLine(isWrong);
            return isWrong == null;
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }


        public const string ReleasePageUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/21";

        public FootLockerBot() : base("Footlocker", "https://www.footlocker.com",
            ReleasePageUrl)
        {
        }
    }
}