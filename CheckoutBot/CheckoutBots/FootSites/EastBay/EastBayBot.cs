using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Factory;
using CheckoutBot.Models.Checkout;
using EO.Internal;
using EO.WebBrowser;
using EO.WebEngine;
using OpenQA.Selenium.Support.UI;
using ScraperCore.Http;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using Cookie = EO.WebEngine.Cookie;
using CookieCollection = System.Net.CookieCollection;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {

        public int DelayInSecond { private get; set; } = 2;
        
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";

        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }

        public override HttpClient Login(string username, string password, CancellationToken token)
        {
            Driver.Url = WebsiteBaseUrl;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            var engine = Driver.Engine;
            var cookieCollector = new CookieCollection();
            ScriptCallDoneHandler callHandler = (sender, args) =>
            {    
                var cookies = engine.CookieManager.GetCookies();
                cookieCollector = new CookieCollection();
                for (int i = 0; i < cookies.Count; i++)
                {
                    var cookie = cookies[i];
                    cookieCollector.Add(new Cookie(cookie.Name, cookie.Value));
                }
            };

            Driver.ScriptCallDone += callHandler;
            
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            

            return null;

        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            var cartContainer = GetClickableElementByXPath("//div[@id = 'header_cart_button']", wait, token);
            cartContainer.Click();

            var select = new SelectElement(GetVisibleElementByXPath("//select[@id = 'billCountry']", wait, token));
            try
            {
                select.SelectByText(settings.Shipping.Country.ToString());
            }
            catch
            {
                Logger.Instance.WriteErrorLog("This country isn't available");
            }


            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Driver.Url = this.WebsiteBaseUrl;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            throw new NotImplementedException();
        }

        public static string GetScriptByXpath(string xPath)
        {
            return
                $@"document.evaluate(""{xPath}"", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }
    }
}