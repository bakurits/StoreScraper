using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.Internal;
using EO.WebBrowser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ScraperCore.Interfaces;
using StoreScraper.Attributes;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;
using static OpenQA.Selenium.Support.UI.ExpectedConditions;


namespace CheckoutBot.CheckoutBots.FootSites
{
    [DisableInGUI]
    public abstract partial class FootSitesBotBase : IWebsiteScraper, IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
    {
        public string WebsiteName { get; set; }
        public string WebsiteBaseUrl { get; set; }
        private string ReleasePageApiEndpoint { get; set; }
        public WebView Driver { get; set; }

        protected FootSitesBotBase(string websiteName, string webSiteBaseUrl, string releasePageEndpoint)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = webSiteBaseUrl;
            this.ReleasePageApiEndpoint = releasePageEndpoint;
        }


        public virtual void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {

        }

        public virtual void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            
        }

      
        public List<FootsitesProduct> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(("Accept","application/json")).AddHeaders(ClientFactory.FirefoxUserAgentHeader);
            var task = client.GetStringAsync(ReleasePageApiEndpoint);
            task.Wait(token);

            if (task.IsFaulted) throw new JsonException("Can't get data");

            var productData = Utils.GetFirstJson(task.Result).GetValue("releases");
            var products = GetProducts(productData);
            return products;
        }

        private List<FootsitesProduct> GetProducts(JToken data)
        {
            var products = new List<FootsitesProduct>();
            foreach (var day in data)
            {
                var productsOnDay = day["products"];
                foreach (var productData in productsOnDay)
                    try
                    {
                        var date = GetDate(productData);
                        var name = GetPropertyAsString(productData, "name");
                        var price = GetPrice(productData);
                        var url = GetUrl(productData);
                        var image = GetPropertyAsString(productData, "primaryImageURL");
                        var sku = GetPropertyAsString(productData, "sku");
                        var model = GetPropertyAsString(productData, "model");
                        var color = GetPropertyAsString(productData, "color");
                        var showLaunchCountdown = GetCountDownEnabled(productData);
                        var gender = GetGender(productData);
                        

                        var product = new FootsitesProduct(this, name, url, price, image, url, "USD", date)
                        {
                            Sku = sku,
                            Model = model,
                            Color = color,
                            Gender = gender,
                            LaunchCountdownEnabled = (showLaunchCountdown != 0)
                        };

                        products.Add(product);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
            }

            return products;
        }

        private double GetPrice(JToken productData)
        {
            if (productData["price"].Type != JTokenType.Null)
                return (double) productData["price"];
            return 0;
        }

        private string GetUrl(JToken productData)
        {
            if (productData["buyNowURL"].Type != JTokenType.Null)
                return ((string) productData["buyNowURL"]).StringBefore("/?sid=");
            return "Not Available";
        }

        private DateTime GetDate(JToken productData)
        {
            if (productData["launchDateTimeUTC"].Type == JTokenType.Null) return DateTime.MaxValue;
            var dateInJson = (string) productData["launchDateTimeUTC"];
            var date = DateTime.Parse(dateInJson);
            return date;

        }
        
        private string GetPropertyAsString(JToken productData, string property)
        {
            if (productData[property].Type != JTokenType.Null)
                return (string) productData[property];
            return "Not Available";
        }
        
        private int GetCountDownEnabled(JToken productData)
        {
            if (productData["showLaunchCountdown"].Type != JTokenType.Null)
                return (int) productData["showLaunchCountdown"];
            return 1;
        }

        private string GetGender(JToken productData)
        {
            if (productData["availableSizes"].Type == JTokenType.Null) return "Not Available";
            if (((JArray)productData["availableSizes"])[0]["gender"].Type != JTokenType.Null)
                return (string) ((JArray)productData["availableSizes"])[0]["gender"];
            return "Not Available";
        }


        protected static IWebElement GetVisibleElementByXPath(string xPath, WebDriverWait wait, CancellationToken token)
        {
            var element = wait.Until(ElementIsVisible(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }

        protected static IWebElement GetClickableElementByXPath(string xPath, WebDriverWait wait, CancellationToken token)
        {
            var element = wait.Until(ElementToBeClickable(By.XPath(xPath)));
            token.ThrowIfCancellationRequested();
            return element;
        }

        protected static string GetScriptByXpath(string xPath)
        {
            return
                $@"document.evaluate(""{xPath}"", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
        }


        protected static string GetAjaxRequest(JObject data, string url, string method)
        {
            return "";
        }

        public override string ToString()
        {
            return this.WebsiteName;
        }
    }
}
