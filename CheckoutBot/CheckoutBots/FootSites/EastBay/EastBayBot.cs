using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CheckoutBot.Controls;
using CheckoutBot.Core;
using CheckoutBot.Interfaces;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using EO.Internal;
using EO.WebBrowser;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StoreScraper.Core;
using StoreScraper.Helpers;
using StoreScraper.Http.Factory;

namespace CheckoutBot.CheckoutBots.FootSites.EastBay
{
    public class EastBayBot : FootSitesBotBase, IProxyChecker
    {
        private const string ApiUrl = "http://pciis02.eastbay.com/api/v2/productlaunch/ReleaseCalendar/1";
        private const string CartUrl = "https://www.eastbay.com/shoppingcart";
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
            var webView = Browser.ActiveTab;
            webView.LoadUrl(WebsiteBaseUrl);
            Task.Delay(5 * 1000, token).Wait(token);
            webView.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            

            webView.EvalScript($"{GetScriptByXpath("//input[@id='login_email']")}.value=\"{username}\"");
            webView.EvalScript($"{GetScriptByXpath("//input[@id='login_password']")}.value=\"{password}\"");
            webView.EvalScript($"{GetScriptByXpath("//input[@id='login_submit']")}.click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            string isWrong = (string) webView.EvalScript(@"document.getElementById(""login_password_error"").innerHTML");
            //Console.WriteLine(isWrong);
            return isWrong.Length == 0;
        }

        public override void GuestCheckOut(GuestCheckoutSettings settings, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        
        public override void AccountCheckout(AccountCheckoutSettings settings, CancellationToken token)
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
            RemoveAllItems(Browser.ActiveTab, token);
            Logger.Instance.WriteVerboseLog($"Cart cleared!", Color.DarkOliveGreen);
            Logger.Instance.WriteVerboseLog($"Preparing for checkout...", Color.Black);
            FootsitesProduct arbitraryProduct = GetArbitraryItem(token);
            Browser.ActiveTab.LoadUrlAndWait(arbitraryProduct.Url);
            Task.Delay(4000, token).Wait(token);
            AddArbitraryItemToCart(arbitraryProduct, token);
            Task.Delay(2000, token).Wait(token);
            GoToCheckoutPage(token);
            var cartTab = Browser.NewTab("Cart");
            RemoveArbitraryItem(cartTab, arbitraryProduct, token);
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
            FinalCheckout(settings,token);
            Logger.Instance.WriteVerboseLog("CHECKOUT SUCCESS!!!", Color.DarkGreen);
            Logger.Instance.WriteVerboseLog("Signing out from account...");
            LogOut(token);
            Logger.Instance.WriteVerboseLog("Sign out success!", Color.Green);
        }
        
        /// <summary>
        /// This method adds product to cart
        /// </summary>
        /// <param name="settings"> Checkout settings </param>
        /// <param name="token"></param>
        private void AddToCart(AccountCheckoutSettings settings, CancellationToken token)
        { 
            Browser.ActiveTab.QueueScriptCall(AjaxGetRequest($@"'https://www.eastbay.com/pdp/gateway?requestKey=' +
                            requestKey +
                            '&action=add&qty={settings.BuyOptions.Quantity}&sku={settings.ProductToBuy.Sku}&size={settings.BuyOptions.Size}&fulfillmentType=SHIP_TO_HOME&storeNumber=0&_=' +
                            date")).WaitOne();

            Task.Delay(2000, token).Wait(token);
        }


        protected void FinalCheckout(AccountCheckoutSettings settings,CancellationToken token)
        {
            Browser.ActiveTab.QueueScriptCall($@"document.getElementById(""payMethodPaneStoredCCCVV"").value = ""{settings.UserCcv2}""").WaitOne();
            Task.Delay(10 * 1000, token).Wait(token);
            Browser.ActiveTab.QueueScriptCall(@"document.getElementById(""orderSubmit"").click()").WaitOne();
            Task.Delay(10 * 1000, token).Wait(token);
        }

        /// <summary>
        /// This method gets random item from releases page
        /// </summary>
        private FootsitesProduct GetArbitraryItem(CancellationToken token)
        {
            FootsitesProduct product = ScrapeReleasePage(token)[0];
            GetProductSizes(product, token);
            return product;
        }
        
        /// <summary>
        /// This method adds Arbitrary item to cart
        /// </summary>
        /// <param name="product"> Item to add </param>
        /// <param name="token"></param>
        private void AddArbitraryItemToCart(FootsitesProduct product , CancellationToken token)
        {
            AddToCart(new AccountCheckoutSettings()
            {
                ProductToBuy = product,
                BuyOptions = new ProductBuyOptions()
                {
                    Quantity = 1,
                    Size = product.Sizes[0]
                }
            }, token);
          
        }



        private void GoToCheckoutPage(CancellationToken token)
        {
            Browser.ActiveTab.LoadUrl(CartUrl);
            Task.Delay(4000, token).Wait(token);
            Browser.ActiveTab.QueueScriptCall("document.getElementById(\"cart_checkout_button\").click();").WaitOne(); //load checkout page
            Debug.WriteLine(Browser.ActiveTab.LastJSException);
            Task.Delay(5000, token).Wait(token);
        }

        /// <summary>
        /// Navigates to cart page and removes arbitrary product from cart
        /// </summary>
        /// <param name="driver"> driver from which scripts are called </param>
        /// <param name="product"> product to remove </param>
        /// <param name="token"></param>
        private void RemoveArbitraryItem(WebView driver, FootsitesProduct product, CancellationToken token)
        {
            driver.LoadUrlAndWait(CartUrl);
            Task.Delay(4000, token).Wait(token);
            Debug.WriteLine(GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']/div/span/div/a[@data-btntype = 'remove']/span[2]") + ".click()");
            driver.QueueScriptCall(GetScriptByXpath("//div[@id = 'cart_items']/ul/li[@data-sku = '" + product.Sku + "']/div/span/div/a[@data-btntype = 'remove']/span[2]") + ".click()").WaitOne();
            /*driver.EvalScript($@"
                    var xhr = new XMLHttpRequest();
                    var date = Date.now();
                    var requestKey = window.frames.accountGateway._requestKey;
                    var itemId = {lineItemIdScript}
                    xhr.open('GET', 'https://www.eastbay.com/pdp/gateway?requestKey=' + requestKey + '&action=delete&lineItemId='+ itemId + '&_=' + date);
                    xhr.onload = function() {{
                        if (xhr.status === 200) {{
                            console.log(xhr.responseText);
                        }}
                        else {{
                            alert('Request failed.  Returned status of ' + xhr.status);
                        }}
                    }};
                    xhr.send();
            ");*/
        }


        /// <summary>
        /// Blocks current thread until product will be released
        /// </summary>
        /// <param name="model">unique model code of product to wait until release</param>
        /// <param name="token"></param>
        private void WaitBeforeRelease(string model, CancellationToken token)
        {
            bool released = false;

            do
            {
                var allReleases = ScrapeReleasePage(token);
                released = !allReleases.Find(p => p.Model == model)?.LaunchCountdownEnabled?? false;
                Task.Delay(25, token).Wait(token);
            } while (!released);
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
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="proxyPool"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public List<WebProxy> ChooseBestProxies(List<WebProxy> proxyPool, int maxCount)
        {
            List<Tuple<long, WebProxy>> lst = new List<Tuple<long, WebProxy>>();
            Parallel.ForEach(proxyPool, proxy =>
            {
                using (HttpClient client = ClientFactory.CreateProxiedHttpClient(proxy, true).AddHeaders(ClientFactory.DefaultHeaders))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    
                    try
                    {
                        var doc = client.GetDoc(WebsiteBaseUrl, CancellationToken.None);
                        stopwatch.Stop();
                        if (!IsFaulted(doc))
                        {
                            lst.Add(new Tuple<long, WebProxy>(stopwatch.ElapsedMilliseconds, proxy));   
                        }    
                    }
                    catch
                    {
                        // ignored
                    }
                }
            });

            lst = lst.OrderBy(item => item.Item1).ToList().GetRange(0, Math.Min( maxCount, lst.Count));
            List<WebProxy> ans = new List<WebProxy>();
            foreach (var item in lst)
            {
                ans.Add(item.Item2);
                Console.WriteLine(item.Item1);
            }
            
            return ans;
        }

        private bool IsFaulted(HtmlDocument doc)
        {
            var ind = doc.DocumentNode.SelectSingleNode("//title").InnerHtml.IndexOf("Access Denied", StringComparison.Ordinal);
            return ind != -1;
        }

        private void RemoveAllItems(WebView webView, CancellationToken token)
        {
            webView.LoadUrlAndWait(CartUrl);
            Task.Delay(4000, token).Wait(token);
            webView.QueueScriptCall(
                @"var items = document.getElementById(""cart_items"").getElementsByTagName(""ul"")[0].getElementsByTagName(""li"")""
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    item.querySelectorAll('[title=""Remove Item From Cart""]')[0].click();
    
                }").WaitOne();
            Task.Delay(2000, token).Wait(token);
        }

        private void LogOut(CancellationToken token)
        {
            var webView = Browser.ActiveTab;
            webView.LoadUrl(WebsiteBaseUrl);
            Task.Delay(5 * 1000, token).Wait(token);
            webView.EvalScript(GetScriptByXpath("//div[@id='header_account_button']/a/span") + ".click();");

            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
            webView.EvalScript("document.getElementById(\"header_registered_logout_link\").click()");
            Task.Delay(DelayInSecond * 1000, token).Wait(token);
            
        }
    }
}