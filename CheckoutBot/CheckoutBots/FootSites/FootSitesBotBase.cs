﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using CheckoutBot.Controls;
using CheckoutBot.Core;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.Internal;
using EO.WebBrowser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScraperCore.Interfaces;
using StoreScraper.Attributes;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;
using StoreScraper.Models;


namespace CheckoutBot.CheckoutBots.FootSites
{
    [DisableInGUI]
    public abstract  class FootSitesBotBase : IWebsiteScraper, IGuestCheckouter, IAccountCheckouter, IReleasePageScraper
    {
        public string WebsiteName { get; set; }
        public string WebsiteBaseUrl { get; set; }
        private string ReleasePageApiEndpoint { get; set; }
        //public WebView Driver { get; set; }
        //public WebView DriverForArbitraryProduct { get; set; }
        //public WebView Driver3 { get; set; }

        [ThreadStatic] 
        public static EOBrowserDriver Browser;

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

        public abstract bool Login(string username, string password, CancellationToken token);


        public List<FootsitesProduct> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.CreateHttpClient(autoCookies: true).AddHeaders(("Accept","application/json")).AddHeaders(ClientFactory.FirefoxUserAgentHeader)
                .AddHeaders(("Accept-Language", "en-US,en; q=0.5"));
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
                        if(productData["availableInventory"].Value<int>() == 0) continue;

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
            {   
                string s = ((string) productData["buyNowURL"]).StringBefore("/?sid=");
                Uri uri = new Uri(s);
                string correctedUri = this.WebsiteBaseUrl + "/" + uri.PathAndQuery;
                return correctedUri;
            } 
            return null;
        }

        private DateTime GetDate(JToken productData)
        {
            if (productData["launchDateTimeUTC"].Type == JTokenType.Null) return DateTime.MaxValue;
            var dateInJson = productData["launchDateTimeUTC"].Value<DateTime>();
            var date = dateInJson;
            return date;

        }
        
        private string GetPropertyAsString(JToken productData, string property)
        {
            if (productData[property].Type != JTokenType.Null)
                return (string) productData[property];
            return null;
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


        protected static string GetScriptByXpath(string xPath)
        {
            return
                $@"document.evaluate(""{xPath}"", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue";
        }


        protected static string AjaxGetRequest(string url)
        {
            return $@"
                        var xhr = new XMLHttpRequest();
                        var date = Date.now();
                        xhr.open('GET', {url});
                        xhr.onload = function() {{
                            if (xhr.status === 200) {{
                                console.log(xhr.responseText);
                            }} else {{
                                alert('Request failed.  Returned status of ' + xhr.status);
                            }}
                        }};
                        xhr.send();";
        }

        public override string ToString()
        {
            return this.WebsiteName;
        }
    }
}
