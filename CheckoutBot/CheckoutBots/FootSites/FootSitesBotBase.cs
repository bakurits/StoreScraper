using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Core;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.WebBrowser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScraperCore.Interfaces;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;


namespace CheckoutBot.CheckoutBots.FootSites
{
    [JsonObject]
    public abstract  class FootSitesBotBase : IWebsiteScraper, IGuestCheckouter, IAccountCheckouter, IReleasePageScraper, IBrowserSession
    {
        public string WebsiteName { get;}
        public string WebsiteBaseUrl { get; }
        private string ReleasePageApiEndpoint { get; set; }

        [JsonIgnore]
        public EOBrowserDriver Browser;

        protected FootSitesBotBase(string websiteName, string webSiteBaseUrl, string releasePageEndpoint)
        {
            this.WebsiteName = websiteName;
            this.WebsiteBaseUrl = webSiteBaseUrl;
            this.ReleasePageApiEndpoint = releasePageEndpoint;
        }

        public void Start(bool hidden = false, string proxy = null)
        {
            Browser = new EOBrowserDriver(proxy);
            Task.Factory.StartNew(() => Browser.ShowDialog(), TaskCreationOptions.LongRunning);
            Task.Delay(4000).Wait();
        }


        public virtual void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {

        }

        public virtual void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
        {
            Logger.Instance.WriteVerboseLog($"Checkout process ({settings.ProductToBuy.Name}) started", Color.DarkOrange);
            Browser.NewTab("MainTab");

            Logger.Instance.WriteVerboseLog($"Signing in (username={settings.UserLogin}...");
            if (!Login(settings.UserLogin, settings.UserPassword, token))
            {
                Logger.Instance.WriteErrorLog("Wrong password");
            }
            Logger.Instance.WriteVerboseLog($"Login successful!", Color.DarkOliveGreen);

            Logger.Instance.WriteVerboseLog($"Clearing cart...", Color.Black);
            RemoveAllItems(token);
            Logger.Instance.WriteVerboseLog($"Cart cleared!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog($"Preparing for checkout...", Color.Black);
            FootsitesProduct arbitraryProduct = GetArbitraryItem(token);
            Browser.ActiveTab.LoadUrlAndWait(arbitraryProduct.Url);
            Task.Delay(4000, token).Wait(token);
            AddArbitraryItem(arbitraryProduct, token);
            Task.Delay(2000, token).Wait(token);
            GoToCheckoutPage(token);
            Browser.NewTab("Cart");
            RemoveArbitraryItem(arbitraryProduct, token);
            Browser.ActiveTab.LoadUrlAndWait(settings.ProductToBuy.Url);
            Task.Delay(4000, token).Wait(token);
            Logger.Instance.WriteVerboseLog("Preparation Done!", Color.DarkOliveGreen);
            var secondsLeft = (settings.ProductToBuy.ReleaseTime - DateTime.UtcNow)?.Seconds;
            Logger.Instance.WriteVerboseLog($"Waiting product to be released (about {secondsLeft} seconds left...");

            WaitBeforeRelease(settings.ProductToBuy.Model, token);
            Logger.Instance.WriteVerboseLog($"Product release detected!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog("Adding product to cart..", Color.Black);
            AddToCart(settings, token);
            Logger.Instance.WriteVerboseLog("Product successfully added to cart!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog("Checkout product...", Color.Black);
            Browser.SwitchToTab(0).Reload().WaitOne();
            FinalCheckout(settings, token);
            Logger.Instance.WriteVerboseLog("CHECKOUT SUCCESS!!!", Color.DarkGreen);
            Logger.Instance.WriteVerboseLog("Signing out from account...");
            LogOut(token);
            Logger.Instance.WriteVerboseLog("Sign out success!", Color.Green);
            
        }

        protected abstract void AddArbitraryItem(FootsitesProduct product, CancellationToken token);
        protected abstract void RemoveArbitraryItem(FootsitesProduct product, CancellationToken token);
        protected abstract void GoToCheckoutPage(CancellationToken token);
        protected abstract void WaitBeforeRelease(string model, CancellationToken token);
        protected abstract void AddToCart(AccountCheckoutSettings settings, CancellationToken token);
        protected abstract void FinalCheckout(AccountCheckoutSettings settings, CancellationToken token);
        protected abstract void LogOut(CancellationToken token);
        protected abstract void RemoveAllItems(CancellationToken token);
        protected abstract FootsitesProduct GetArbitraryItem(CancellationToken token);
        
        
        

        public abstract bool Login(string username, string password, CancellationToken token);


        public List<FootsitesProduct> ScrapeReleasePage(CancellationToken token)
        {
            var proxy = Helper.GetRandomProxy(this);
            var client = ClientFactory.CreateHttpClient(proxy:proxy,autoCookies: true).AddHeaders(("Accept","application/json")).AddHeaders(ClientFactory.FirefoxUserAgentHeader)
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
                                DONE = true;
                                console.log(xhr.responseText);
                            }} else {{
                                alert('Request failed.  Returned status of ' + xhr.status);
                            }}
                        }};
                        xhr.send();";
        }

        protected static void ImitateTyping(WebView webView, string xPath, string str, CancellationToken token)
        {
            webView.QueueScriptCall($"{GetScriptByXpath(xPath)}.focus()").WaitOne();
            webView.QueueScriptCall($"{GetScriptByXpath(xPath)}.select()").WaitOne();
            foreach (var key in str)
            {
                webView.SendChar(key);
                Task.Delay(25, token).Wait(token);
            }

            //Task.Delay(1000, token).Wait(token);
        }

        public override string ToString()
        {
            return this.WebsiteName;
        }

        public override bool Equals(object obj)
        {
            return obj != null && this.GetType() == obj.GetType();
        }

        public override int GetHashCode()
        {
            return this.WebsiteBaseUrl.GetHashCode();
        }

        public void Stop()
        {
            this.Browser.Dispose();
        }
    }
}
