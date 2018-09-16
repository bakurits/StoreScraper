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
`           Driver2.LoadUrlAndWait(WebsiteBaseUrl);
            Driver.LoadUrlAndWait(WebsiteBaseUrl);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");
            

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            /*var engine = Driver.Engine;
            var cookieCollector = new CookieCollection();

            void CallHandler(object sender, ScriptCallDoneEventArgs args)
            {
                var cookies = engine.CookieManager.GetCookies();
                cookieCollector = new CookieCollection();
                for (int i = 0; i < cookies.Count; i++)
                {
                    var cookie = cookies[i];
                    cookieCollector.Add(new Cookie(cookie.Name, cookie.Value));
                }
            }

            Driver.ScriptCallDone += CallHandler;
            */

            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.EvalScript($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            string isWrong = (string) Driver.EvalScript(@"document.getElementById(""login_password_error"").innerHTML");
            //Console.WriteLine(isWrong);
            return isWrong.Length == 0;

            /*Console.WriteLine("ylep");
            
            
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(cookieCollector);

            var handler  = new ExtendedClientHandler()
            {
                UseCookies = true,
                MaxAutomaticRedirections = 3,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = true,
                CookieContainer = cookieContainer
            };

            var client = new HttpClient(handler).AddHeaders(ClientFactory.ChromeHeaders);
            client.Timeout = TimeSpan.FromSeconds(5);

            var doc = client.GetDoc("https://www.eastbay.com", CancellationToken.None);
            Driver.LoadHtml(doc.DocumentNode.InnerHtml);
            */

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
            AddArbitraryItemToCart(token);
            AddToCart(settings, token);

        }

        private void AddArbitraryItemToCart(CancellationToken token)
        {
            AccountCheckoutSettings settings =
                new AccountCheckoutSettings()
                {
                    UserLogin = "bakuricucxashvili@gmail.com",
                    UserPassword = "Yrf7B2RHW",
                    UserCcv2 = "123",
                    ProductToBuy = new FootsitesProduct(new FootSimpleBase.EastBayScraper(), "ADIDAS TEAM STRUCTURED FLEX CAP - MEN'S",
                        "https://www.eastbay.com/product/model:295115/sku:M038Z013/adidas-team-structured-flex-cap-mens/all-white/white/",
                        12, "", "M038Z013"),
                    BuyOptions = new ProductBuyOptions()
                    {
                        Size = "XS/S"
                    }
                };
           AddToCart(settings,token);
        }

        private void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
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