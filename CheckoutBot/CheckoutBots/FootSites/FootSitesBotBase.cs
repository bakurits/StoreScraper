﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CheckoutBot.Interfaces;
using CheckoutBot.Models.Checkout;
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

      
        public List<Product> ScrapeReleasePage(CancellationToken token)
        {
            var client = ClientFactory.GetProxiedFirefoxClient(autoCookies: true);
            var task = client.GetStringAsync(ReleasePageApiEndpoint);
            task.Wait(token);

            if (task.IsFaulted) throw new JsonException("Can't get data");

            var productData = Utils.GetFirstJson(task.Result).GetValue("releases");
            var products = GetProducts(productData);
            return products;
        }

        private List<Product> GetProducts(JToken data)
        {
            var products = new List<Product>();
            foreach (var day in data)
            {
                var productsOnDay = day["products"];
                foreach (var productData in productsOnDay)
                    try
                    {
                        var date = GetDate(productData);
                        if (date < DateTime.UtcNow) continue;
                        var name = GetName(productData);
                        var price = GetPrice(productData);
                        var url = GetUrl(productData);
                        var image = GetImage(productData);
                        

                        var product = new Product(this, name, url, price, image, url, "USD", date);

                        products.Add(product);
                    }
                    catch
                    {
                        Debug.WriteLine(productData);
                    }
            }

            return products;
        }

        private string GetName(JToken productData)
        {
            if (productData["name"].Type != JTokenType.Null)
                return (string) productData["name"];
            return "Not Available";
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

        private string GetImage(JToken productData)
        {
            if (productData["primaryImageURL"].Type != JTokenType.Null)
                return (string) productData["primaryImageURL"];
            return "Not Available";
        }

        private DateTime GetDate(JToken productData)
        {
            if (productData["launchDateTimeUTC"].Type == JTokenType.Null) return DateTime.MaxValue;
            var dateInJson = (string) productData["launchDateTimeUTC"];
            var date = DateTime.Parse(dateInJson);
            return date;

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
    }
}
