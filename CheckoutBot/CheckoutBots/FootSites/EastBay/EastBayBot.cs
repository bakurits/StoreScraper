﻿using System;
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
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";

        public EastBayBot() : base("EastBay", "https://www.eastbay.com", ApiUrl)
        {
        }


        public EastBayBot(string websiteName, string webSiteBaseUrl, string releasePageEndpoint) : base(websiteName,
            webSiteBaseUrl, releasePageEndpoint)
        {
        }

        public int DelayInSecond { private get; set; } = 2;

        public override void Login(string username, string password, CancellationToken token)
        {
            Driver.Url = WebsiteBaseUrl;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            var engine = Driver.Engine;
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
            
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            Console.WriteLine("ylep");
            
            
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
            
            return;
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
            Driver.Url = settings.ProductToBuy.Url;
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{settings.UserLogin}\"");
            Driver.QueueScriptCall(
                $"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{settings.UserPassword}\"");
            Driver.QueueScriptCall($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            string getReq = $@"
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
xhr.send();";
            // Console.WriteLine(getReq);
            Driver.EvalScript(getReq);


        }
    }
}