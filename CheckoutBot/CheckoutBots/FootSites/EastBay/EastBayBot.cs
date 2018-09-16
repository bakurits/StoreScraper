using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Factory;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.Internal;
using EO.WebBrowser;
using EO.WebEngine;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.UI;
using ScraperCore.Http;
using StoreScraper.Bots.Sticky_bit.ChampsSports_EastBay;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using Cookie = EO.WebEngine.Cookie;
using CookieCollection = System.Net.CookieCollection;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";

        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }

        public int DelayInSecond { private get; set; } = 2;

        public override bool Login(string username, string password, CancellationToken token)
        {
            Driver2.LoadUrlAndWait(WebsiteBaseUrl);
            Driver.LoadUrlAndWait(WebsiteBaseUrl);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");
            

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            

            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            string isWrong = (string) Driver.EvalScript(@"document.getElementById(""login_password_error"").innerHTML");
            //Console.WriteLine(isWrong);
            return isWrong.Length == 0;
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            var driver = DriverFactory.CreateFirefoxDriver();
            driver.Navigate().GoToUrl(WebsiteBaseUrl);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            

            throw new NotImplementedException();
        }

        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Login(settings.UserLogin, settings.UserPassword, token);
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            AddToCart(settings, token);

        }

        public void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
        {
            Driver.LoadUrlAndWait(settings.ProductToBuy.Url);
            Driver.EvalScript($@"
var xhr = new XMLHttpRequest();
var date = Date.now();
xhr.open('GET',
    'https://www.eastbay.com/pdp/gateway?requestKey=' +
    requestKey +
    '&action=add&qty=1&sku={settings.ProductToBuy.Id}&size={settings.BuyOptions.Size}&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=' +
    date);
xhr.onload = function() {{
    if (xhr.status === 200) {{
        console.log(xhr.responseText);
    }} else {{
        alert('Request failed.  Returned status of ' + xhr.status);
    }}
}};
xhr.send();");
        }


        /// <summary>
        /// Blocks current thread until product will be released
        /// </summary>
        /// <param name="model">unique model code of product to wait until release</param>
        private void WaitBeforeRelease(string model, CancellationToken token)
        {
            bool released = false;

            do
            {
                var allReleases = ScrapeReleasePage(token);
                released = !allReleases.Find(p => p.Model == model).LaunchCountdownEnabled;
                Task.Delay(25, token).Wait(token);
            } while (released);
        }

        public void GetProductSizes(FootsitesProduct product, CancellationToken token)
        {
            List<string> infos = new List<string>();
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(ClientFactory.FireFoxHeaders);
            var document = client.GetDoc(product.Url, token).DocumentNode;
            int ind = document.InnerHtml.IndexOf("var styles = ", StringComparison.Ordinal);
            var sizeData = Utils.GetFirstJson(document.InnerHtml.Substring(ind));
            var sizesForCurProd = (JArray)sizeData[product.Sku][7];

            foreach (var item in sizesForCurProd)
            {
                var t = (JArray) item;
                var s = (string)t[0];
                infos.Add(s.Trim());
            }

            product.Sizes = infos;
        }
    }
}